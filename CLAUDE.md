# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

QuikytLoader is an Avalonia UI desktop application for downloading YouTube videos as MP3 files and sending them to Telegram. Built for .NET 9 targeting Linux (Arch), following MVVM architecture with clean separation of concerns.

## Build and Run Commands

```bash
# Build the project
dotnet build

# Run the application
dotnet run

# Build for release (self-contained)
dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true

# Output location after publish:
# bin/Release/net9.0/linux-x64/publish/QuikytLoader
```

## External Dependencies

- **yt-dlp**: Must be installed on system (`sudo pacman -S yt-dlp` on Arch)
- Used by YouTubeDownloadService to download and convert YouTube videos to MP3

## Architecture Overview

### MVVM Pattern with Navigation

The app uses a layered navigation system:

1. **AppViewModel** (App.axaml.cs:27) - Root ViewModel managing navigation between views
   - Injects HomeViewModel and SettingsViewModel
   - Handles view switching via NavigateToHome/NavigateToSettings commands
   - Maintains selected tab state

2. **HomeViewModel** - Main YouTube download functionality
   - Orchestrates download workflow via DownloadAndSendAsync command
   - Uses IYouTubeDownloadService for downloads
   - Progress reporting via IProgress<double>
   - Status management and validation

3. **SettingsViewModel** - Telegram bot configuration (planned)
   - Will store bot token and chat ID

### Service Layer

**YouTubeDownloadService** - Core download logic
- Downloads to system temp directory (`/tmp/QuikytLoader`)
- Moves final MP3 to `~/Downloads/QuikytLoader`
- Embeds comprehensive metadata (Artist, Album, Composer, Performer, Publisher, etc.)
- Embeds video thumbnail as album art with automatic format conversion
- Handles duplicate files with numbered suffixes: `filename (1).mp3`, `filename (2).mp3`
- Auto-cleanup of temporary files (thumbnails, metadata) after processing
- Spawns yt-dlp process with arguments built in BuildYtDlpArguments (YouTubeDownloadService.cs:98)
- Parses progress from yt-dlp output via regex (YouTubeDownloadService.cs:316)

### Dependency Injection

All services and ViewModels registered in App.axaml.cs:
- AppViewModel, HomeViewModel, SettingsViewModel (Transient)
- IYouTubeDownloadService -> YouTubeDownloadService (Singleton)
- ITelegramBotService (planned, currently commented out)

Constructor injection used throughout - never use ServiceProvider directly.

### UI Structure

MainWindow contains:
- 80px vertical sidebar (Firefox/Zen-style navigation)
- Tab buttons for Home and Settings
- ContentControl bound to AppViewModel.CurrentView for dynamic view switching

## Key Design Patterns

- **Single Responsibility**: Each method has one clear purpose (e.g., ValidateUrl, UpdateProgress, CleanupTempFiles)
- **Command Pattern**: CommunityToolkit.Mvvm's [RelayCommand] for UI actions
- **Law of Demeter**: No deep property chains, clear interfaces between layers
- **Progress Reporting**: IProgress<double> pattern for async operations

## Important Implementation Notes

### File Handling
- YouTubeDownloadService uses sanitized video titles as filenames via `%(title)s` template
- Downloads happen in temp directory first, then moved to final location
- Temp directory: `/tmp/QuikytLoader`
- Final directory: `~/Downloads/QuikytLoader`
- Automatic cleanup of thumbnails and metadata files after processing

### yt-dlp Integration
- All yt-dlp arguments constructed in BuildYtDlpArguments method
- Metadata mapping: YouTube fields -> MP3 ID3 tags
- Progress extracted via regex from stdout/stderr
- Comprehensive metadata embedding including Artist, Album, Composer, Publisher, etc.
- Thumbnail embedding with automatic format conversion to JPG

### MVVM Communication
- ViewModels never reference Views directly
- Commands expose functionality to UI via data binding
- Observable properties ([ObservableProperty]) auto-generate INotifyPropertyChanged
- Command availability controlled via CanExecute predicates

## Planned Features (Not Yet Implemented)

- ITelegramBotService integration (Telegram.Bot NuGet package already included)
- Settings persistence for bot token and chat ID
- The workflow currently simulates Telegram sending with Task.Delay (HomeViewModel.cs:138)
