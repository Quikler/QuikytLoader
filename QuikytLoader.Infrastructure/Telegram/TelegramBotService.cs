using QuikytLoader.Application.Interfaces.Settings;
using QuikytLoader.Application.Interfaces.Services;
using QuikytLoader.Domain.Common;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace QuikytLoader.Infrastructure.Telegram;

internal class TelegramBotService(IUserSettings userSettings) : ITelegramBotService
{
    private TelegramBotClient? _botClient;
    private CancellationTokenSource? _cts;
    private string? _currentBotToken;
    private string? _currentChatId;

    public async Task<Result> SendAudioAsync(string audioFilePath, string thumbnailPath)
    {
        var initResult = await EnsureInitializedAsync();
        if (!initResult.IsSuccess) return initResult;

        if (!File.Exists(audioFilePath)) return Errors.Telegram.AudioFileNotFound(audioFilePath);
        if (!long.TryParse(_currentChatId, out var chatIdValue)) return Errors.Telegram.InvalidChatIdFormat(_currentChatId);

        try
        {
            await using var audioStream = File.OpenRead(audioFilePath);
            var audioFileName = Path.GetFileName(audioFilePath);
            var audioInputFile = InputFile.FromStream(audioStream, audioFileName);

            try
            {
                await using var thumbnailStream = File.OpenRead(thumbnailPath);
                var thumbnailFileName = Path.GetFileName(thumbnailPath);
                var thumbnailInputFile = InputFile.FromStream(thumbnailStream, thumbnailFileName);

                await _botClient!.SendAudio(
                    chatId: new ChatId(chatIdValue),
                    audio: audioInputFile,
                    thumbnail: thumbnailInputFile,
                    cancellationToken: _cts?.Token ?? CancellationToken.None
                );

                Console.WriteLine($"Audio file sent to Telegram: {audioFileName}");

                return Result.Success();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex) when (ex.GetType().Namespace?.StartsWith("Telegram.Bot") == true)
            {
                return Errors.Telegram.SendFailed(ex.Message);
            }
        }
        catch (IOException ex)
        {
            return Errors.Telegram.FileReadError(audioFilePath, thumbnailPath, ex.Message);
        }
    }

    /// <summary>
    /// Ensures the Telegram bot client is initialized
    /// Reloads settings on each call to pick up configuration changes
    /// </summary>
    private async Task<Result> EnsureInitializedAsync()
    {
        var settings = userSettings.Load();

        if (string.IsNullOrWhiteSpace(settings.BotToken))
            return Errors.Telegram.BotTokenNotConfigured();

        var tokenChanged = _currentBotToken != settings.BotToken;

        if (_botClient != null && !tokenChanged)
        {
            _currentChatId = settings.ChatId;
            return Result.Success();
        }

        DisposeInternal();

        _currentBotToken = settings.BotToken;
        _currentChatId = settings.ChatId;
        _botClient = new TelegramBotClient(_currentBotToken);
        _cts = new CancellationTokenSource();

        try
        {
            var me = await _botClient.GetMe(_cts.Token);
            Console.WriteLine($"Telegram bot initialized: @{me.Username}");
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex.GetType().Namespace?.StartsWith("Telegram.Bot") == true)
        {
            return Errors.Telegram.InitializationFailed(ex.Message);
        }
    }

    private void DisposeInternal()
    {
        if (_botClient is null) return;

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        _botClient = null;
        _currentBotToken = null;
        _currentChatId = null;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        DisposeInternal();
    }
}
