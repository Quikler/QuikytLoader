using QuikytLoader.Application.DTOs;
using QuikytLoader.Application.Interfaces.Queue;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;
using QuikytLoader.Domain.Enums;

namespace QuikytLoader.Application.UseCases;

public class AddToQueueUseCase(FindExistingDownloadUseCase findExistingDownloadUseCase, IYtDlpService ytDlpService, IDownloadQueue queue, IYoutubeExtractorService youtubeExtractorService)
{
    public async Task<AddToQueueResult> ExecuteAsync(
        string youtubeUrl,
        bool editTitleBeforeDownload,
        bool ignoreDuplicateCheck = false)
    {
        return ytDlpService.IsPlaylist(youtubeUrl)
            ? await AddPlaylistAsync(youtubeUrl)
            : await AddSingleAsync(youtubeUrl, editTitleBeforeDownload, ignoreDuplicateCheck);
    }

    private async Task<AddToQueueResult> AddSingleAsync(string youtubeUrl, bool editTitleBeforeDownload, bool ignoreDuplicateCheck)
    {
        if (!ignoreDuplicateCheck)
        {
            var duplicateCheck = await findExistingDownloadUseCase.FindAsync(youtubeUrl);
            if (!duplicateCheck.IsSuccess)
                return new AddToQueueResult.Failed(duplicateCheck.Error);
            if (duplicateCheck.Value is not null)
                return new AddToQueueResult.DuplicateDetected(duplicateCheck.Value);
        }

        var videoIdResult = youtubeExtractorService.GetVideoId(youtubeUrl);
        if (!videoIdResult.IsSuccess)
            return new AddToQueueResult.Failed(videoIdResult.Error);

        var queueItem = new QueueItem
        {
            // Appling Source now in order to show `Url` in UI
            Source = new DownloadSource(youtubeUrl, videoIdResult.Value),
            // Assigning null explicitly since we are loading metadata synchronously
            Metadata = null,
            Status = editTitleBeforeDownload ? DownloadStatus.Editing : DownloadStatus.Pending,
        };

        // Checking if queueItem with same SourceId is already queued BEFORE calling queue.EnqueueItem
        if (queue.ContainsSourceId(queueItem.Source.SourceId))
            return new AddToQueueResult.AlreadyQueued();

        queue.EnqueueItem(queueItem);
        _ = EnrichAsync(queueItem);

        return new AddToQueueResult.SingleAdded(queue.ItemsCount);
    }

    /// <summary>
    /// This method gets video metadata and updates it for already queued item
    /// </summary>
    private async Task EnrichAsync(QueueItem queueItem)
    {
        var metadataResult = await ytDlpService.GetVideoMetadataAsync(queueItem.Source.Url);
        if (!metadataResult.IsSuccess)
        {
            queue.UpdateItem(queueItem with
            {
                Status = DownloadStatus.Failed,
                Error = metadataResult.Error
            });
            return;
        }

        if (!metadataResult.Value.IsAvailable)
        {
            queueItem.Status = DownloadStatus.Disabled;
            queueItem.Error = new Error(metadataResult.Value.UnavailableReason);
        }

        queue.UpdateItem(queueItem with
        {
            Metadata = metadataResult.Value
        });
    }

    private async Task<AddToQueueResult> AddPlaylistAsync(string youtubeUrl)
    {
        var playlistMetadataResult = await ytDlpService.GetPlaylistMetadataAsync(youtubeUrl, 15);
        if (!playlistMetadataResult.IsSuccess)
            return new AddToQueueResult.Failed(playlistMetadataResult.Error);
        var playlistMetadata = playlistMetadataResult.Value;

        if (queue.ContainsGroup(playlistMetadata.PlaylistId)) return new AddToQueueResult.AlreadyQueued();

        List<QueueItem> items = [];
        HashSet<string> seenSourceIds = [];
        foreach (var playlistVideo in playlistMetadata.PlaylistVideos)
        {
            var queueItem = new QueueItem
            {
                Source = playlistVideo.Source,
                Metadata = playlistVideo.Metadata,
                Status = DownloadStatus.Queued,
            };

            var duplicateCheck = await findExistingDownloadUseCase.FindAsync(playlistVideo.Source.Url);
            if (!duplicateCheck.IsSuccess)
            {
                queueItem.Error = duplicateCheck.Error;
            }
            else if (duplicateCheck.Value is not null)
            {
                queueItem.Status = DownloadStatus.Disabled;
                queueItem.Error = new Error("Already downloaded");
            }
            else if (!playlistVideo.Metadata.IsAvailable)
            {
                queueItem.Status = DownloadStatus.Disabled;
                queueItem.Error = new Error(playlistVideo.Metadata.UnavailableReason);
            }
            else if (queue.ContainsSourceId(queueItem.Source.SourceId))
            {
                queueItem.Status = DownloadStatus.Disabled;
                queueItem.Error = new Error("Already queued");
            }
            else if (!seenSourceIds.Add(queueItem.Source.SourceId))
            {
                queueItem.Status = DownloadStatus.Disabled;
                queueItem.Error = new Error("Duplicate video in playlist");
            }

            items.Add(queueItem);
        }

        var queueGroup = new QueueGroup(
            playlistMetadata.PlaylistId,
            playlistMetadata.PlaylistTitle,
            [.. items.Select(i => i.Id)]);

        queue.EnqueueGroup(queueGroup, items);

        return new AddToQueueResult.PlaylistAdded(
            playlistMetadata.PlaylistTitle,
            items.Count);
    }
}

public abstract record AddToQueueResult
{
    public sealed record Failed(Error Error) : AddToQueueResult;
    public sealed record DuplicateDetected(DownloadHistoryDto ExistingDownload) : AddToQueueResult;
    public sealed record AlreadyQueued() : AddToQueueResult;
    public sealed record SingleAdded(int QueueCount) : AddToQueueResult;
    public sealed record PlaylistAdded(string PlaylistTitle, int ItemCount) : AddToQueueResult;
}
