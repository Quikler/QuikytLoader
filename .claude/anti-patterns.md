# Anti-Patterns

This document lists anti-patterns to avoid in QuikytLoader. Each section shows wrong vs right patterns with code examples.

---

## Architecture Violations

### Never reference Infrastructure from AvaloniaUI

**Wrong:**
```csharp
// In QuikytLoader.AvaloniaUI
using QuikytLoader.Infrastructure.Services;
using QuikytLoader.Infrastructure.Telegram;
```

**Right:**
```csharp
// In QuikytLoader.AvaloniaUI
using QuikytLoader.Application.Interfaces.Services;
```

### Never reference external layers from Domain

**Wrong:**
```csharp
// In QuikytLoader.Domain
using QuikytLoader.Application.DTOs;
using QuikytLoader.Infrastructure.Persistence;
```

**Right:**
```csharp
// Domain has no external dependencies - only System namespaces
using System;
using System.Text.RegularExpressions;
```

### Never reference Infrastructure from Application

**Wrong:**
```csharp
// In QuikytLoader.Application
using QuikytLoader.Infrastructure.YouTube;
```

**Right:**
```csharp
// Application defines interfaces, Infrastructure implements them
namespace QuikytLoader.Application.Interfaces.Services;
public interface IYtDlpService { }
```

---

## DI Anti-Patterns

### Never use IServiceProvider outside Startup

**Wrong:**
```csharp
// In ViewModel or Service
public class HomeViewModel
{
    public HomeViewModel(IServiceProvider provider)
    {
        _ytDlpService = provider.GetRequiredService<IYtDlpService>();
    }
}
```

**Right:**
```csharp
// Direct constructor injection
public class HomeViewModel
{
    public HomeViewModel(IYtDlpService ytDlpService)
    {
        _ytDlpService = ytDlpService;
    }
}
```

### Never create services with `new`

**Wrong:**
```csharp
public class DownloadAndSendUseCase
{
    private readonly IYtDlpService _ytDlpService = new YtDlpService();
}
```

**Right:**
```csharp
public class DownloadAndSendUseCase
{
    private readonly IYtDlpService _ytDlpService;

    public DownloadAndSendUseCase(IYtDlpService ytDlpService)
    {
        _ytDlpService = ytDlpService;
    }
}
```

### Never change DI lifetimes without understanding implications

- **Singleton**: One instance for entire app lifetime (e.g., TelegramBotService, UserSettings)
- **Transient**: New instance per request (e.g., UseCases, ViewModels)

**Wrong:**
```csharp
// Making a stateful service Transient causes state loss
services.AddTransient<IUserSettings, UserSettings>();
```

**Right:**
```csharp
// Stateful services that cache data should be Singleton
services.AddSingleton<IUserSettings, UserSettings>();
```

---

## MVVM Anti-Patterns

### Never reference Views from ViewModels

**Wrong:**
```csharp
// In ViewModel
using QuikytLoader.AvaloniaUI.Views;

public class HomeViewModel
{
    private MainWindow _window;

    public void ShowDialog()
    {
        _window.ShowDialog();
    }
}
```

**Right:**
```csharp
// Use IDialogService abstraction
public class HomeViewModel
{
    private readonly IDialogService _dialogService;

    public async Task ShowErrorAsync(string message)
    {
        await _dialogService.ShowErrorAsync(message);
    }
}
```

### Never put business logic in Views/code-behind

**Wrong:**
```csharp
// In MainWindow.axaml.cs
private async void DownloadButton_Click(object sender, EventArgs e)
{
    var result = await _ytDlpService.DownloadAsync(url);
    if (result.IsSuccess)
    {
        await _telegramService.SendAsync(result.Value);
    }
}
```

**Right:**
```csharp
// View only binds to ViewModel commands
// MainWindow.axaml
<Button Command="{Binding DownloadCommand}" />

// ViewModel uses [RelayCommand] attribute (CommunityToolkit.Mvvm)
// Generates DownloadCommand property, instantiated once
[RelayCommand]
private void ExecuteDownload()
{
    // ... logic ...
}
```

### Never manipulate UI directly in ViewModels

**Wrong:**
```csharp
public class HomeViewModel
{
    public void UpdateUI()
    {
        Dispatcher.UIThread.Post(() =>
        {
            _progressBar.Value = 50;
        });
    }
}
```

**Right:**
```csharp
public class HomeViewModel : ViewModelBase
{
    private int _progress;
    public int Progress
    {
        get => _progress;
        set => this.RaiseAndSetIfChanged(ref _progress, value);
    }
}
```

---

## Result Pattern Misuse

### Never throw exceptions for expected failures

**Wrong:**
```csharp
public YouTubeUrl Parse(string url)
{
    if (!IsValidYouTubeUrl(url))
        throw new ArgumentException("Invalid YouTube URL");

    return new YouTubeUrl(url);
}
```

**Right:**
```csharp
public Result<YouTubeUrl> Parse(string url)
{
    if (!IsValidYouTubeUrl(url))
        return Result<YouTubeUrl>.Failure(Errors.YouTube.InvalidUrl);

    return Result<YouTubeUrl>.Success(new YouTubeUrl(url));
}
```

### Never access Value without checking IsSuccess

**Wrong:**
```csharp
var result = await _useCase.ExecuteAsync(url);
var filePath = result.Value.FilePath; // May throw if failed
```

**Right:**
```csharp
var result = await _useCase.ExecuteAsync(url);
if (!result.IsSuccess)
{
    HandleError(result.Error);
    return;
}
var filePath = result.Value.FilePath;
```

### Never use null instead of Result for failure cases

**Wrong:**
```csharp
public DownloadResultDto? Download(string url)
{
    if (!IsValid(url))
        return null; // Caller doesn't know why it failed
}
```

**Right:**
```csharp
public Result<DownloadResultDto> Download(string url)
{
    if (!IsValid(url))
        return Result<DownloadResultDto>.Failure(Errors.YouTube.InvalidUrl);
}
```

---

## Value Object Misuse

### Never use raw strings for YouTube IDs/URLs

**Wrong:**
```csharp
public async Task<Result<string>> DownloadAsync(string youtubeUrl)
{
    var videoId = ExtractVideoId(youtubeUrl); // string
}
```

**Right:**
```csharp
public async Task<Result<string>> DownloadAsync(YouTubeUrl url)
{
    YouTubeId videoId = url.VideoId; // Strongly typed
}
```

### Never bypass factory methods

**Wrong:**
```csharp
// Using reflection or internal access to bypass validation
var url = new YouTubeUrl(rawString);
```

**Right:**
```csharp
// Always use factory methods that validate
var result = YouTubeUrl.TryParse(rawString);
if (result.IsSuccess)
{
    var url = result.Value;
}
```

---

## File Handling Anti-Patterns

### Never save files to user directories

**Wrong:**
```csharp
var outputPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    "Downloads",
    "video.mp3"
);
```

**Right:**
```csharp
// Use system temp directory (cross-platform), send to Telegram, then cleanup
var tempDir = Path.Combine(Path.GetTempPath(), "QuikytLoader");
Directory.CreateDirectory(tempDir); // Ensures directory exists
var outputPath = Path.Combine(tempDir, $"{videoId}.mp3");
// After sending to Telegram:
File.Delete(outputPath);
```

### Never skip temp file cleanup

**Wrong:**
```csharp
public async Task ProcessAsync()
{
    var tempFile = await DownloadToTempAsync();
    await SendToTelegramAsync(tempFile);
    // Missing cleanup - temp files accumulate
}
```

**Right:**
```csharp
public async Task ProcessAsync()
{
    var tempFile = await DownloadToTempAsync();
    try
    {
        await SendToTelegramAsync(tempFile);
    }
    finally
    {
        if (File.Exists(tempFile))
            File.Delete(tempFile);
    }
}
```

---

## General C# Anti-Patterns

### God Class
Classes with too many responsibilities. Split into focused classes.

**Wrong:**
```csharp
public class DownloadManager
{
    public void Download() { }
    public void Convert() { }
    public void SendToTelegram() { }
    public void SaveToDatabase() { }
    public void ValidateUrl() { }
    public void ParseSettings() { }
}
```

**Right:**
```csharp
// Separate classes with single responsibility
public class YtDlpService { } // Download + Convert
public class TelegramBotService { } // Send
public class DownloadHistoryRepository { } // Database
public class ValidateYouTubeUrlUseCase { } // Validation
```

### Magic Strings/Numbers
Unexplained literal values. Use constants or configuration.

**Wrong:**
```csharp
if (videoId.Length != 11) { }
var timeout = 30000;
var pattern = @"(?:v=|\/shorts\/|youtu\.be\/)([a-zA-Z0-9_-]{11})";
```

**Right:**
```csharp
private const int YouTubeVideoIdLength = 11;
private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
private static readonly Regex YouTubeUrlPattern = new(@"...", RegexOptions.Compiled);
```

### Swallowing Exceptions
Catching exceptions without handling or logging.

**Wrong:**
```csharp
try
{
    await DownloadAsync();
}
catch (Exception)
{
    // Silent failure - impossible to debug
}
```

**Right:**
```csharp
try
{
    await DownloadAsync();
}
catch (Exception ex)
{
    _logger.LogError(ex, "Download failed for {Url}", url);
    return Result<DownloadResultDto>.Failure(Errors.Download.Failed);
}
```

### Premature Optimization
Optimizing before measuring.

**Wrong:**
```csharp
// Complex caching without evidence it's needed
private readonly ConcurrentDictionary<string, WeakReference<VideoInfo>> _cache;
```

**Right:**
```csharp
// Simple implementation first, optimize when profiling shows need
private readonly Dictionary<string, VideoInfo> _cache;
```

### Leaky Abstractions
Interfaces that expose implementation details.

**Wrong:**
```csharp
public interface IYtDlpService
{
    Process StartYtDlpProcess(string args); // Exposes Process
    string GetYtDlpPath(); // Exposes file path
}
```

**Right:**
```csharp
public interface IYtDlpService
{
    Task<Result<string>> DownloadAudioAsync(YouTubeUrl url, ...);
    Task<Result<string>> GetVideoTitleAsync(YouTubeUrl url, ...);
}
```
