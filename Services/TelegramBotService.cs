using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using QuikytLoader.Models;

namespace QuikytLoader.Services;

/// <summary>
/// Service for managing Telegram bot operations
/// Handles sending audio files to configured Telegram chat
/// </summary>
public class TelegramBotService(ISettingsManager settingsManager) : ITelegramBotService
{
    private TelegramBotClient? _botClient;
    private CancellationTokenSource? _cts;
    private AppSettings? _currentSettings;
    private bool _isInitialized;

    /// <summary>
    /// Sends an audio file to the configured Telegram chat with optional thumbnail
    /// Automatically initializes the bot on first use (lazy initialization)
    /// </summary>
    /// <returns>The Telegram message ID of the sent message</returns>
    public async Task<int> SendAudioAsync(string audioFilePath, string? thumbnailPath = null)
    {
        // Ensure bot is initialized (lazy initialization)
        await EnsureInitializedAsync();

        if (_currentSettings == null || string.IsNullOrWhiteSpace(_currentSettings.ChatId))
        {
            throw new InvalidOperationException("Chat ID is not configured. Please set it in Settings.");
        }

        if (!File.Exists(audioFilePath))
        {
            throw new FileNotFoundException($"Audio file not found: {audioFilePath}");
        }

        var chatId = new ChatId(long.Parse(_currentSettings.ChatId));

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

            var message = await _botClient!.SendAudio(
                chatId: chatId,
                audio: audioInputFile,
                thumbnail: thumbnailInputFile,
                cancellationToken: _cts?.Token ?? CancellationToken.None
            );

            Console.WriteLine($"Audio file sent to Telegram: {fileName}" +
                            (thumbnailInputFile != null ? " (with thumbnail)" : "") +
                            $" (Message ID: {message.Id})");

            return message.Id;
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
    /// </summary>
    private async Task EnsureInitializedAsync()
    {
        // Always reload settings to pick up changes made in Settings page
        var settings = settingsManager.Load();

        if (string.IsNullOrWhiteSpace(settings.BotToken))
        {
            throw new InvalidOperationException("Bot token is not configured. Please set it in Settings.");
        }

        // If bot token changed, need to recreate client
        var tokenChanged = _currentSettings?.BotToken != settings.BotToken;

        if (_isInitialized && !tokenChanged)
        {
            // Already initialized with same token, just update settings
            _currentSettings = settings;
            return;
        }

        // Dispose existing resources if reinitializing
        if (_isInitialized)
        {
            await DisposeInternalAsync();
        }

        _currentSettings = settings;
        _botClient = new TelegramBotClient(_currentSettings.BotToken);
        _cts = new CancellationTokenSource();

        // Verify bot connection
        var me = await _botClient.GetMe(_cts.Token);
        Console.WriteLine($"Telegram bot initialized: @{me.Username}");

        _isInitialized = true;
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
        _currentSettings = null;
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
        Console.WriteLine("Telegram bot disposed");
    }
}
