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

3. **SettingsViewModel** - Telegram bot configuration
   - Manages bot token and chat ID via ISettingsManager
   - Provides save/load commands for settings persistence

### Service Layer

**YouTubeDownloadService** - Core download logic
- Downloads to system temp directory (`/tmp/QuikytLoader`) only - files not saved to user's Downloads
- All files remain in temp directory for sending to Telegram, then cleaned up by HomeViewModel
- Embeds comprehensive metadata (Artist, Album, Composer, Performer, Publisher, etc.)
- Embeds video thumbnail as album art with automatic format conversion
- Auto-normalizes filenames (removes extra whitespace)
- Spawns yt-dlp process with arguments built in BuildYtDlpArguments (YouTubeDownloadService.cs:185)
- Parses progress from yt-dlp output via regex
- Supports custom title overrides via GetVideoTitleAsync and custom DownloadAsync overload
- Returns DownloadResult with YouTubeId, TempMediaFilePath and TempThumbnailPath
- Uses IYoutubeExtractor to extract video IDs from URLs

**YoutubeExtractor** - YouTube ID extraction
- Fast regex-based extraction for common YouTube URL formats (youtube.com/watch?v=ID, youtu.be/ID, etc.)
- Fallback to yt-dlp `--print id` for edge cases
- Returns 11-character YouTube video ID

**TelegramBotService** - Telegram integration
- Lazy initialization pattern: bot client created on first SendAudioAsync call
- Reloads settings on each send to pick up configuration changes
- Sends MP3 files with optional thumbnail to configured chat ID
- Returns Telegram message ID after successful send
- Uses Telegram.Bot library (v22.7.5)
- Implements IAsyncDisposable for proper cleanup on app shutdown

**DownloadHistoryService** - Download history tracking
- Stores YouTube download history in SQLite database
- Checks for duplicate downloads by YouTube ID
- Saves download records with video title, timestamp, Telegram message ID, and thumbnail URL
- Uses INSERT OR REPLACE for upserts (updates DownloadedAt timestamp on re-downloads)
- Fetches thumbnail URLs via yt-dlp or falls back to YouTube CDN

**DbConnectionService** - Database connection management
- Manages SQLite database at `~/.config/QuikytLoader/history.db`
- SQLite automatically creates database file on first connection
- Initializes schema with CREATE TABLE IF NOT EXISTS (idempotent, safe for concurrent calls)
- Sets restrictive file permissions on Linux (mode 600 - user read/write only)
- Provides connections to other services (currently used by DownloadHistoryService)

**SettingsManager** - JSON-based settings persistence
- Stores config in `~/.config/QuikytLoader/settings.json` (follows XDG Base Directory spec)
- Atomic writes via temp file + rename to prevent corruption
- Sets restrictive file permissions on Linux (mode 600 - user read/write only)
- Auto-creates default settings if file doesn't exist or is corrupted

### Dependency Injection

All services and ViewModels registered in App.axaml.cs:
- AppViewModel, HomeViewModel, SettingsViewModel, MainWindowViewModel (Transient)
- ISettingsManager -> SettingsManager (Singleton)
- IYouTubeDownloadService -> YouTubeDownloadService (Singleton)
- ITelegramBotService -> TelegramBotService (Singleton)
- IYoutubeExtractor -> YoutubeExtractor (Singleton)
- IDbConnectionService -> DbConnectionService (Singleton)
- IDownloadHistoryService -> DownloadHistoryService (Singleton)

Constructor injection used throughout - never use ServiceProvider directly.
App shutdown handler calls host.StopAsync() to properly dispose async disposable services.

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

### Download Queue System
- HomeViewModel maintains ObservableCollection<DownloadQueueItem> for batch downloads
- Queue processes sequentially: downloads next pending item, sends to Telegram, marks completed
- Each queue item tracks its own status (Pending/Downloading/Completed/Failed/Cancelled)
- Per-item progress reporting via IProgress<double> bound to queue item
- Queue processing runs in background (_isQueueProcessing flag prevents duplicate processing)
- Temp files (media + thumbnail) automatically cleaned up after each queue item completes

### Custom Title Editing Workflow
- Two-step process when EditTitle checkbox is checked:
  1. First click "Add to Queue": fetches video title via GetVideoTitleAsync, populates CustomTitle field
  2. User edits title, clicks "Proceed": adds to queue with custom title
- Button text dynamically changes: "Add to Queue" -> "Proceed"
- IsProceedButtonState flag tracks button state for UI styling
- State resets when: URL changes, EditTitle unchecked, or item added to queue

### File Handling and Cleanup
- YouTubeDownloadService uses sanitized video titles as filenames via `%(title)s` template
- Custom titles sanitized via SanitizeFilename (replaces invalid chars with underscore)
- All downloads stored in temp directory: `/tmp/QuikytLoader`
- Files NOT saved to user's Downloads folder - only temporary for Telegram upload
- Thumbnail processing: crops to square, resizes to 320x320 max for Telegram requirements
- HomeViewModel handles cleanup: deletes both media file and thumbnail after sending to Telegram
- Cleanup happens in finally block to ensure temp files removed even on errors
- DownloadResult contains TempMediaFilePath and TempThumbnailPath properties

### yt-dlp Integration
- All yt-dlp arguments constructed in BuildYtDlpArguments method
- Metadata mapping: YouTube fields -> MP3 ID3 tags
- Progress extracted via regex from stdout/stderr
- Comprehensive metadata embedding including Artist, Album, Composer, Publisher, etc.
- Thumbnail embedding with automatic format conversion to JPG
- Process cancellation supported: kills yt-dlp process tree on CancellationToken

### MVVM Communication
- ViewModels never reference Views directly
- Commands expose functionality to UI via data binding
- Observable properties ([ObservableProperty]) auto-generate INotifyPropertyChanged
- Command availability controlled via CanExecute predicates
- NotifyCanExecuteChanged() called when conditions change (e.g., URL validity, processing state)

### Download History and Duplicate Detection
- HomeViewModel checks for duplicates before adding to queue using IDownloadHistoryService
- Duplicate detection extracts YouTube ID via IYoutubeExtractor and queries SQLite database
- Currently logs duplicate warning to console (UI dialog for user confirmation to be implemented)
- After successful Telegram send, saves record to history with:
  - YouTube video ID (11 chars, primary key)
  - Video title (custom or original from filename)
  - Download timestamp (ISO 8601 UTC format)
  - Telegram message ID (for future retrieval)
  - Thumbnail URL (from yt-dlp or YouTube CDN fallback)
- Re-downloading same video updates the DownloadedAt timestamp (INSERT OR REPLACE)
- Database stored at `~/.config/QuikytLoader/history.db`
- Schema: DownloadHistory table with YouTubeId as primary key

### Settings and Security
- Settings stored in JSON at `~/.config/QuikytLoader/settings.json`
- Database stored at `~/.config/QuikytLoader/history.db`
- File permissions restricted to mode 600 on Linux (user read/write only) for security
- TelegramBotService validates settings on each send (throws if BotToken or ChatId missing)
- Bot token can be obtained from @BotFather on Telegram
- Chat ID can be obtained from @userinfobot on Telegram
