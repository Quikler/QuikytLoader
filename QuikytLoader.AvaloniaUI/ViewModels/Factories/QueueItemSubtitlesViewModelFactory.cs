using QuikytLoader.Application.Interfaces.Settings;
using QuikytLoader.Application.UseCases;
using QuikytLoader.AvaloniaUI.ViewModels.Queue.QueueEntry.Subtitles;
using QuikytLoader.Domain.Entities;

namespace QuikytLoader.AvaloniaUI.ViewModels.Factories;

public class QueueItemSubtitlesViewModelFactory(
    IUserSettings userSettings,
    IFetchManualSubtitlesUseCase fetchManualSubtitlesUseCase,
    IFetchAutoSubtitlesUseCase fetchAutoSubtitlesUseCase,
    ICancelSubtitlesUseCase cancelSubtitlesUseCase)
{
    public QueueItemSubtitlesViewModel Create(Subtitles subtitles)
        => new(subtitles,
            userSettings,
            fetchManualSubtitlesUseCase,
            fetchAutoSubtitlesUseCase,
            cancelSubtitlesUseCase);
}
