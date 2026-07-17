using QuikytLoader.Application.DTOs;
using QuikytLoader.Application.Interfaces.Parsers;
using QuikytLoader.Application.Interfaces.Queue;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;
using QuikytLoader.Domain.Entities;
using QuikytLoader.Domain.Enums;

namespace QuikytLoader.Application.UseCases;

public class AddToQueueUseCase(
    FindExistingDownloadUseCase findExistingDownloadUseCase,
    IDownloadQueue queue,
    IYoutubeMetadataService youtubeMetadataService,
    IYoutubeVideoIdParser youtubeVideoIdParser,
    IYoutubePlaylistIdParser youtubePlaylistIdParser)
{
    public async Task<AddToQueueResult> ExecuteAsync(
        string youtubeUrl,
        bool editTitleBeforeDownload,
        bool ignoreDuplicateCheck = false)
    {
        // Firstly try to parse playlist - &list= query url param
        var youtubePlaylistIdResult = youtubePlaylistIdParser.Parse(youtubeUrl);
        if (youtubePlaylistIdResult.IsSuccess)
            return await AddPlaylistAsync(new DownloadPlaylistSource(youtubeUrl, youtubePlaylistIdResult.Value));

        // Secondly try to parse a single video
        var youtubeVideoIdResult = youtubeVideoIdParser.Parse(youtubeUrl);
        if (youtubeVideoIdResult.IsSuccess)
            return await AddSingleAsync(new DownloadSource(youtubeUrl, youtubeVideoIdResult.Value), editTitleBeforeDownload, ignoreDuplicateCheck);

        return new AddToQueueResult.Failed(youtubeVideoIdResult.Error);
    }

    private async Task<AddToQueueResult> AddSingleAsync(DownloadSource downloadSource, bool editTitleBeforeDownload, bool ignoreDuplicateCheck)
    {
        if (!ignoreDuplicateCheck)
        {
            var duplicateCheck = await findExistingDownloadUseCase.FindByIdAsync(downloadSource.YoutubeVideoId);
            if (!duplicateCheck.IsSuccess)
                return new AddToQueueResult.Failed(duplicateCheck.Error);
            if (duplicateCheck.Value is not null)
                return new AddToQueueResult.DuplicateDetected(duplicateCheck.Value);
        }

        // Checking if same SourceId is already queued BEFORE calling queue.EnqueueItem
        if (queue.ContainsSourceId(downloadSource.YoutubeVideoId))
            return new AddToQueueResult.AlreadyQueued(downloadSource.YoutubeVideoId);

        var queueItem = new QueueItem
        {
            Source = downloadSource,
            // Assigning null explicitly since metadata will be loaded later
            Metadata = null,
            Status = editTitleBeforeDownload ? DownloadStatus.Editing : DownloadStatus.Pending,
        };

        queue.EnqueueItem(queueItem);
        _ = EnrichAsync(queueItem);

        return new AddToQueueResult.SingleAdded(queue.ItemsCount);
    }

    /// <summary>
    /// This method gets video metadata and updates it for already queued item
    /// </summary>
    private async Task EnrichAsync(QueueItem queueItem)
    {
        var metadataResult = await youtubeMetadataService.GetVideoMetadataAsync(queueItem.Source);
        if (!metadataResult.IsSuccess)
        {
            queueItem.Status = DownloadStatus.Failed;
            queueItem.Error = metadataResult.Error;
            queue.UpdateItem(queueItem.Id);
            return;
        }

        queueItem.Metadata = metadataResult.Value;
        queue.UpdateItem(queueItem.Id);
    }

    private async Task<AddToQueueResult> AddPlaylistAsync(DownloadPlaylistSource downloadPlaylistSource)
    {
        if (queue.ContainsGroup(downloadPlaylistSource.YoutubePlaylistId))
            return new AddToQueueResult.PlaylistAlreadyQueued(downloadPlaylistSource.YoutubePlaylistId);

        var playlistMetadataResult = await youtubeMetadataService.GetPlaylistMetadataAsync(downloadPlaylistSource, 15);
        if (!playlistMetadataResult.IsSuccess)
            return new AddToQueueResult.Failed(playlistMetadataResult.Error);
        var playlistMetadata = playlistMetadataResult.Value;

        var duplicateChecks = await findExistingDownloadUseCase.FindMultipleAsync(playlistMetadata.PlaylistVideos);

        List<QueueItem> items = [];
        HashSet<string> seenSourceIds = [];
        foreach (var (playlistVideo, duplicateCheck) in duplicateChecks)
        {
            var queueItem = new QueueItem
            {
                Source = playlistVideo.Source,
                Metadata = playlistVideo.Metadata,
                Status = DownloadStatus.Queued,
            };

            if (!duplicateCheck.IsSuccess)
            {
                queueItem.Status = DownloadStatus.Failed;
                queueItem.Error = duplicateCheck.Error;
            }
            else if (duplicateCheck.Value is not null)
            {
                queueItem.Status = DownloadStatus.Disabled;
                queueItem.Error = new Error("Already downloaded");
            }
            else if (queue.ContainsSourceId(queueItem.Source.YoutubeVideoId))
            {
                queueItem.Status = DownloadStatus.Disabled;
                queueItem.Error = new Error("Already queued");
            }
            else if (!seenSourceIds.Add(queueItem.Source.YoutubeVideoId))
            {
                queueItem.Status = DownloadStatus.Disabled;
                queueItem.Error = new Error("Duplicate video in playlist");
            }

            items.Add(queueItem);
        }

        var queueGroup = new QueueGroup(
            downloadPlaylistSource.YoutubePlaylistId,
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
    public sealed record AlreadyQueued(string VideoId) : AddToQueueResult;
    public sealed record PlaylistAlreadyQueued(string PlaylistId) : AddToQueueResult;
    public sealed record SingleAdded(int QueueCount) : AddToQueueResult;
    public sealed record PlaylistAdded(string PlaylistTitle, int ItemCount) : AddToQueueResult;
}
