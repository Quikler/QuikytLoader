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
        string youtubeUrl,
        bool editTitleBeforeDownload,
        Func<DownloadHistoryDto, Task<bool>> confirmDuplicate)
    {
        var duplicateCheck = await findExistingDownloadUseCase.FindAsync(youtubeUrl);
        if (!duplicateCheck.IsSuccess)
            return new QueueAdditionResult.Failed(duplicateCheck.Error.Message);

        if (duplicateCheck.Value is not null)
        {
            var proceed = await confirmDuplicate(duplicateCheck.Value);
            if (!proceed) return new QueueAdditionResult.DuplicateCancelled();
        }

        var item = new DownloadQueueItem
        {
            Status = editTitleBeforeDownload ? DownloadStatus.Editing : DownloadStatus.Pending,
        };

        // Apply Url manually to show it in UI
        item.VideoMetadata.Url = youtubeUrl;

        queueManager.EnqueueItem(item);
        _ = getVideoMetadataUseCase.GetMetadataAsync(youtubeUrl)
            .ContinueWith(task => item.ApplyMetadata(task.Result));

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

        var playlistMetadataResult = await getPlaylistMetadataUseCase.GetMetadataAsync(url);
        if (!playlistMetadataResult.IsSuccess)
            return new QueueAdditionResult.Failed(playlistMetadataResult.Error.Message);

        var playlistMetadata = playlistMetadataResult.Value;
        if (playlistMetadata.PlaylistVideos.Count == 0)
            return new QueueAdditionResult.Failed("Playlist has no videos.");

        var downloadQueueItems = CreatePlaylistItemsFromMetadata(playlistMetadata);
        await DisableDuplicateItems(downloadQueueItems);
        queueManager.EnqueueGroup(playlistId, playlistMetadata.PlaylistTitle, downloadQueueItems);

        return new QueueAdditionResult.PlaylistAdded(playlistMetadata.PlaylistTitle, downloadQueueItems.Count);
    }

    private IReadOnlyList<DownloadQueueItem> CreatePlaylistItemsFromMetadata(PlaylistMetadataDto playlistMetadata)
    {
        var items = new List<DownloadQueueItem>(playlistMetadata.PlaylistVideos.Count);
        foreach (var videoMetadataDto in playlistMetadata.PlaylistVideos)
        {
            var downloadQueueItem = new DownloadQueueItem();
            downloadQueueItem.ApplyMetadata(videoMetadataDto);

            if (!videoMetadataDto.IsAvailable)
            {
                downloadQueueItem.SetAsDisabled(videoMetadataDto.UnavailableReason);
            }
            else if (queueManager.IsAlreadyInQueue(videoMetadataDto.VideoId, excludeGroupId: playlistMetadata.PlaylistId))
            {
                downloadQueueItem.SetAsDisabled("Already queued in another playlist");
            }

            items.Add(downloadQueueItem);
        }
        return items;
    }

    private async Task DisableDuplicateItems(IReadOnlyList<DownloadQueueItem> downloadQueueItems)
    {
        var historyTasks = new Dictionary<string, Task<Result<DownloadHistoryDto?>>>();
        foreach (var downloadQueueItem in downloadQueueItems)
        {
            if (!downloadQueueItem.VideoMetadata.IsAvailable) continue;
            if (!historyTasks.ContainsKey(downloadQueueItem.VideoMetadata.VideoId))
                historyTasks[downloadQueueItem.VideoMetadata.VideoId] = findExistingDownloadUseCase.FindByIdAsync(downloadQueueItem.VideoMetadata.VideoId);
        }
        await Task.WhenAll(historyTasks.Values);

        foreach (var downloadQueueItem in downloadQueueItems)
        {
            if (historyTasks.TryGetValue(downloadQueueItem.VideoMetadata.VideoId, out var historyTask)
                 && historyTask.Result.IsSuccess
                 && historyTask.Result.Value is not null)
            {
                downloadQueueItem.SetAsDisabled("Already downloaded");
            }
        }
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
