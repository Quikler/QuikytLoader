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
    /// Sends an audio file to the configured Telegram chat
    /// Automatically initializes the bot on first use (lazy initialization)
    /// </summary>
    public async Task SendAudioAsync(string filePath)
    {
        // Ensure bot is initialized (lazy initialization)
        await EnsureInitializedAsync();

        if (_currentSettings == null || string.IsNullOrWhiteSpace(_currentSettings.ChatId))
        {
            throw new InvalidOperationException("Chat ID is not configured. Please set it in Settings.");
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Audio file not found: {filePath}");
        }

        var chatId = new ChatId(long.Parse(_currentSettings.ChatId));

        await using var fileStream = File.OpenRead(filePath);
        var fileName = Path.GetFileName(filePath);

        var inputFile = InputFile.FromStream(fileStream, fileName);

        await _botClient!.SendAudio(
            chatId: chatId,
            audio: inputFile,
            cancellationToken: _cts?.Token ?? CancellationToken.None
        );

        Console.WriteLine($"Audio file sent to Telegram: {fileName}");
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
