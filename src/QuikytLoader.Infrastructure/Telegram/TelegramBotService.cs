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

    public async Task<Result> SendAudioAsync(string mp3FilePath, string thumbnailFilePath)
    {
        var initResult = await EnsureInitializedAsync();
        if (!initResult.IsSuccess) return initResult;

        if (!long.TryParse(_currentChatId, out var chatIdValue)) return Errors.Telegram.InvalidChatIdFormat(_currentChatId);

        try
        {
            await using var mp3FileStream = File.OpenRead(mp3FilePath);
            await using var thumbnailFileStream = File.OpenRead(thumbnailFilePath);

            await _botClient!.SendAudio(
                chatId: new ChatId(chatIdValue),
                audio: mp3FileStream,
                thumbnail: thumbnailFileStream,
                cancellationToken: _cts?.Token ?? CancellationToken.None
            );

            return Result.Success();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Errors.Telegram.SendFailed(ex.Message);
        }
    }

    /// <summary>
    /// Ensures the Telegram bot client is initialized
    /// Reloads settings on each call to pick up configuration changes
    /// </summary>
    private async Task<Result> EnsureInitializedAsync()
    {
        var settings = userSettings.Current;

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
        _botClient = new TelegramBotClient(_currentBotToken)
        {
            Timeout = TimeSpan.FromMinutes(5)
        };
        _cts = new CancellationTokenSource();

        try
        {
            var me = await _botClient.GetMe(_cts.Token);
            Console.WriteLine($"Telegram bot initialized: @{me.Username}");
            return Result.Success();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
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
