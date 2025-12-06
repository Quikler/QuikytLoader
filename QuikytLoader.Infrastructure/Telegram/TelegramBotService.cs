using QuikytLoader.Application.Interfaces.Repositories;
using QuikytLoader.Application.Interfaces.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace QuikytLoader.Infrastructure.Telegram;

/// <summary>
/// Service for managing Telegram bot operations
/// Handles sending audio files to configured Telegram chat
/// </summary>
internal class TelegramBotService(ISettingsRepository settingsRepository) : ITelegramBotService
{
    private TelegramBotClient? _botClient;
    private CancellationTokenSource? _cts;
    private string? _currentBotToken;
    private string? _currentChatId;
    private bool _isInitialized;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    /// <summary>
    /// Sends an audio file to the configured Telegram chat with optional thumbnail
    /// Automatically initializes the bot on first use (lazy initialization)
    /// </summary>
    public async Task SendAudioAsync(string audioFilePath, string? thumbnailPath = null)
    {
        // Ensure bot is initialized (lazy initialization)
        await EnsureInitializedAsync();

        if (string.IsNullOrWhiteSpace(_currentChatId))
        {
            throw new InvalidOperationException("Chat ID is not configured. Please set it in Settings.");
        }

        if (!File.Exists(audioFilePath))
        {
            throw new FileNotFoundException($"Audio file not found: {audioFilePath}");
        }

        var chatId = new ChatId(long.Parse(_currentChatId));

        await using var audioStream = File.OpenRead(audioFilePath);
        var fileName = Path.GetFileName(audioFilePath);
        var audioInputFile = InputFile.FromStream(audioStream, fileName);

        // Prepare thumbnail if available
        InputFile? thumbnailInputFile = null;
        FileStream? thumbnailStream = null;

        try
        {
            if (thumbnailPath != null && File.Exists(thumbnailPath))
            {
                thumbnailStream = File.OpenRead(thumbnailPath);
                var thumbnailFileName = Path.GetFileName(thumbnailPath);
                thumbnailInputFile = InputFile.FromStream(thumbnailStream, thumbnailFileName);
            }

            await _botClient!.SendAudio(
                chatId: chatId,
                audio: audioInputFile,
                thumbnail: thumbnailInputFile,
                cancellationToken: _cts?.Token ?? CancellationToken.None
            );

            Console.WriteLine($"Audio file sent to Telegram: {fileName}" +
                            (thumbnailInputFile != null ? " (with thumbnail)" : ""));
        }
        finally
        {
            // Clean up thumbnail stream
            if (thumbnailStream != null)
            {
                await thumbnailStream.DisposeAsync();
            }
        }
    }

    /// <summary>
    /// Ensures the Telegram bot client is initialized
    /// Reloads settings on each call to pick up configuration changes
    /// Thread-safe with semaphore to prevent race conditions
    /// </summary>
    private async Task EnsureInitializedAsync()
    {
        await _initLock.WaitAsync();
        try
        {
            // Always reload settings to pick up changes made in Settings page
            var settings = await settingsRepository.LoadAsync();

            if (string.IsNullOrWhiteSpace(settings.BotToken))
            {
                throw new InvalidOperationException("Bot token is not configured. Please set it in Settings.");
            }

            // If bot token changed, need to recreate client
            var tokenChanged = _currentBotToken != settings.BotToken;

            if (_isInitialized && !tokenChanged)
            {
                // Already initialized with same token, just update settings
                _currentChatId = settings.ChatId;
                return;
            }

            // Dispose existing resources if reinitializing
            if (_isInitialized)
            {
                await DisposeInternalAsync();
            }

            _currentBotToken = settings.BotToken;
            _currentChatId = settings.ChatId;
            _botClient = new TelegramBotClient(_currentBotToken);
            _cts = new CancellationTokenSource();

            // Verify bot connection
            var me = await _botClient.GetMe(_cts.Token);
            Console.WriteLine($"Telegram bot initialized: @{me.Username}");

            _isInitialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    /// <summary>
    /// Internal cleanup logic (reusable by both DisposeAsync and reinitialization)
    /// </summary>
    private async ValueTask DisposeInternalAsync()
    {
        if (!_isInitialized)
        {
            return;
        }

        _cts?.Cancel();

        // Give pending operations a moment to cancel gracefully
        await Task.Delay(50);

        _cts?.Dispose();
        _cts = null;
        _botClient = null;
        _currentBotToken = null;
        _currentChatId = null;
        _isInitialized = false;
    }

    /// <summary>
    /// Disposes the Telegram bot client and releases resources
    /// Called automatically by DI container on application shutdown
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await DisposeInternalAsync();
        _initLock.Dispose();
        Console.WriteLine("Telegram bot disposed");
    }
}
