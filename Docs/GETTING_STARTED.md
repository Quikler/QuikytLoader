## Project Structure Overview

```
QuikytLoader/
│
├── Program.cs                    # Entry point
├── App.axaml                     # Application resources
├── App.axaml.cs                  # App initialization + DI setup
│
├── ViewModels/
│   ├── ViewModelBase.cs          # Base class for ViewModels
│   └── MainWindowViewModel.cs    # Main window logic
│
├── Views/
│   ├── MainWindow.axaml          # UI definition
│   └── MainWindow.axaml.cs       # Minimal code-behind
│
└── Services/                     # To be implemented
    ├── IYouTubeDownloadService.cs
    ├── YouTubeDownloadService.cs
    ├── ITelegramBotService.cs
    └── TelegramBotService.cs
```

## Understanding the Current Implementation

### What's Working Now:
✅ UI loads and displays
✅ URL input field
✅ Download button (with validation)
✅ Progress bar
✅ Status messages
✅ Command pattern
✅ MVVM architecture
✅ Dependency Injection setup

### What's Stubbed (To Be Implemented):
🔜 Actual YouTube download (currently simulated)
🔜 Actual Telegram sending (currently simulated)
🔜 Settings/Configuration
🔜 Error handling details

## Next Steps - Implementation Roadmap

### Phase 1: YouTube Download Service (Next!)

**Create the interface:**
```bash
touch Services/IYouTubeDownloadService.cs
```

```csharp
public interface IYouTubeDownloadService
{
    Task<string> DownloadAsync(string url, IProgress<double>? progress = null);
}
```

**Implement the service:**
```bash
touch Services/YouTubeDownloadService.cs
```

This service will:
1. Call yt-dlp via Process.Start()
2. Parse output for progress
3. Return path to downloaded MP3
4. Handle errors

### Phase 2: Telegram Bot Service

**Create the service:**
```bash
touch Services/ITelegramBotService.cs
touch Services/TelegramBotService.cs
```

### Phase 3: Wire Everything Together

Update `MainWindowViewModel.cs`:
```csharp
private readonly IYouTubeDownloadService _youtubeService;
private readonly ITelegramBotService _telegramService;

public MainWindowViewModel(
    IYouTubeDownloadService youtubeService,
    ITelegramBotService telegramService)
{
    _youtubeService = youtubeService;
    _telegramService = telegramService;
}

private async Task DownloadFromYouTubeAsync()
{
    var progress = new Progress<double>(UpdateProgress);
    var filePath = await _youtubeService.DownloadAsync(YoutubeUrl, progress);
}

private async Task SendToTelegramAsync()
{
    await _telegramService.SendAudioAsync(filePath);
}
```

Update `App.axaml.cs` DI registration:
```csharp
services.AddSingleton<IYouTubeDownloadService, YouTubeDownloadService>();
services.AddSingleton<ITelegramBotService, TelegramBotService>();
```

## Building for Production

### Create Self-Contained Executable
```bash
# Linux (Arch)
dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true

# Output location:
# bin/Release/net8.0/linux-x64/publish/QuikytLoader
```

### Install Locally
```bash
# Copy to local bin
sudo cp bin/Release/net8.0/linux-x64/publish/QuikytLoader /usr/local/bin/

# Now run from anywhere:
QuikytLoader
```

### Create Desktop Entry
```bash
# Create .desktop file
cat > ~/.local/share/applications/QuikytLoader.desktop <<EOF
[Desktop Entry]
Type=Application
Name=QuikytLoader
Comment=YouTube to Telegram Downloader
Exec=/usr/local/bin/QuikytLoader
Icon=audio-x-generic
Terminal=false
Categories=AudioVideo;Network;
EOF
```

## Testing the App

### Test URLs
```
Valid:
https://www.youtube.com/watch?v=dQw4w9WgXcQ
https://youtu.be/dQw4w9WgXcQ

Invalid:
https://vimeo.com/123456
not-a-url
```

## Learning Resources

### Avalonia Documentation
- Official Docs: https://docs.avaloniaui.net/
- Tutorials: https://docs.avaloniaui.net/docs/getting-started

### FluentAvalonia
- GitHub: https://github.com/amwx/FluentAvalonia
- Samples: https://github.com/amwx/FluentAvalonia/tree/master/samples

### MVVM with CommunityToolkit
- Docs: https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/
- Source Generators: https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/generators/overview

### Dependency Injection
- MS Docs: https://learn.microsoft.com/dotnet/core/extensions/dependency-injection

### Performance Tips
```bash
# Use AOT compilation for better startup
dotnet publish -c Release -r linux-x64 /p:PublishAot=true
```

## What's Next?

After you get this running, implement:

1. **YouTube Download Service** - Core functionality
2. **Telegram Bot Service** - Send files
3. **Settings Window** - Store bot token
4. **History View** - Track downloads
5. **Themes** - Dark/Light mode
6. **Notifications** - Desktop alerts
7. **Multiple URLs** - Batch downloads
8. **Playlist Support** - Download entire playlists

*"I use Arch btw"* 🐧
