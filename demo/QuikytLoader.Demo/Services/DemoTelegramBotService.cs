using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;

namespace QuikytLoader.Demo.Services;

internal sealed class DemoTelegramBotService : ITelegramBotService
{
    public async Task<Result> SendAudioAsync(
        string mp3FilePath,
        string thumbnailFilePath)
    {
        await Task.Delay(Random.Shared.Next(1000, 2000));
        return Result.Success();
    }

    public void Dispose() { }
}
