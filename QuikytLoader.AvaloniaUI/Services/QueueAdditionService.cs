using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuikytLoader.Application.DTOs;
using QuikytLoader.Application.UseCases;
using QuikytLoader.AvaloniaUI.Models;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Enums;
using QuikytLoader.Domain.ValueObjects;

namespace QuikytLoader.AvaloniaUI.Services;

/// <summary>
/// Orchestrates adding a YouTube URL to the download queue. Dispatches between single-video
/// and playlist flows, applies hard-disable rules, and kicks off parallel metadata fetches.
/// </summary>
public class QueueAdditionService(
    FindExistingDownloadUseCase findExistingDownloadUseCase,
    GetPlaylistMetadataUseCase getPlaylistMetadataUseCase,
    GetVideoMetadataUseCase getVideoMetadataUseCase,
    DownloadQueueManager queueManager)
{
    /// <summary>
    /// Add a URL to the queue. Caller supplies a callback invoked if a single-video URL
    /// matches an existing history record — return true to proceed, false to cancel.
    /// </summary>
    public async Task<QueueAdditionResult> AddAsync(
        string url,
        bool editTitleBeforeDownload,
        Func<DownloadHistoryDto, Task<bool>> confirmDuplicate)
    {
        return YouTubePlaylistUrl.HasPlaylistParam(url)
            ? await AddPlaylistAsync(url)
            : await AddSingleAsync(url, editTitleBeforeDownload, confirmDuplicate);
    }

    private async Task<QueueAdditionResult> AddSingleAsync(
        string url,
        bool editTitleBeforeDownload,
        Func<DownloadHistoryDto, Task<bool>> confirmDuplicate)
    {
        var duplicateCheck = await findExistingDownloadUseCase.FindAsync(url);
        if (!duplicateCheck.IsSuccess)
            return new QueueAdditionResult.Failed(duplicateCheck.Error.Message);

        if (duplicateCheck.Value is not null)
        {
            var proceed = await confirmDuplicate(duplicateCheck.Value);
            if (!proceed) return new QueueAdditionResult.DuplicateCancelled();
        }

        var item = new DownloadQueueItem
        {
            Url = url,
            Status = editTitleBeforeDownload ? DownloadStatus.Editing : DownloadStatus.Pending
        };

        queueManager.Enqueue(item);
        _ = FetchMetadataAsync(item);

        return new QueueAdditionResult.SingleAdded(queueManager.Queue.Count);
    }

    private async Task<QueueAdditionResult> AddPlaylistAsync(string url)
    {
        var playlistUrlResult = YouTubePlaylistUrl.Create(url);
        if (!playlistUrlResult.IsSuccess)
            return new QueueAdditionResult.Failed(playlistUrlResult.Error.Message);

        var playlistId = playlistUrlResult.Value.PlaylistId;
        if (queueManager.HasGroup(playlistId))
            return new QueueAdditionResult.AlreadyQueued();

        var metadataResult = await getPlaylistMetadataUseCase.GetMetadataAsync(url);
        if (!metadataResult.IsSuccess)
            return new QueueAdditionResult.Failed(metadataResult.Error.Message);

        var metadata = metadataResult.Value;
        if (metadata.Entries.Count == 0)
            return new QueueAdditionResult.Failed("Playlist has no videos.");

        var items = await BuildPlaylistItemsAsync(metadata, playlistId);
        queueManager.EnqueueGroup(playlistId, metadata.Title, items);

        return new QueueAdditionResult.PlaylistAdded(metadata.Title, items.Count);
    }

    private async Task<List<DownloadQueueItem>> BuildPlaylistItemsAsync(PlaylistMetadataDto metadata, string playlistId)
    {
        var historyTasks = new Dictionary<string, Task<Result<DownloadHistoryDto?>>>();
        foreach (var entry in metadata.Entries)
        {
            if (!entry.IsAvailable) continue;
            if (!historyTasks.ContainsKey(entry.VideoId))
                historyTasks[entry.VideoId] = findExistingDownloadUseCase.FindByIdAsync(entry.VideoId);
        }
        await Task.WhenAll(historyTasks.Values);

        var items = new List<DownloadQueueItem>(metadata.Entries.Count);
        foreach (var entry in metadata.Entries)
        {
            var item = new DownloadQueueItem
            {
                Url = entry.Url,
                YoutubeId = entry.VideoId,
                VideoTitle = entry.Title,
                ChannelName = entry.Channel,
                Duration = entry.Duration,
                ThumbnailUrl = entry.ThumbnailUrl,
                IsMetadataLoaded = entry.ThumbnailUrl is not null,
                IsSelected = true,
                Status = DownloadStatus.Pending
            };

            if (!entry.IsAvailable)
            {
                item.Disable(entry.UnavailableReason ?? "Unavailable");
            }
            else if (historyTasks.TryGetValue(entry.VideoId, out var histTask)
                     && histTask.Result.IsSuccess
                     && histTask.Result.Value is not null)
            {
                item.Disable("Already downloaded");
            }
            else if (queueManager.TryFindByYoutubeId(entry.VideoId, excludeGroupId: playlistId) is not null)
            {
                item.Disable("Already queued in another playlist");
            }

            items.Add(item);
        }
        return items;
    }

    private async Task FetchMetadataAsync(DownloadQueueItem item)
    {
        var result = await getVideoMetadataUseCase.GetMetadataAsync(item.Url);
        item.ApplyMetadata(result);
    }
}

public abstract record QueueAdditionResult
{
    public sealed record SingleAdded(int QueueCount) : QueueAdditionResult;
    public sealed record PlaylistAdded(string PlaylistTitle, int VideoCount) : QueueAdditionResult;
    public sealed record AlreadyQueued : QueueAdditionResult;
    public sealed record DuplicateCancelled : QueueAdditionResult;
    public sealed record Failed(string Message) : QueueAdditionResult;
}
