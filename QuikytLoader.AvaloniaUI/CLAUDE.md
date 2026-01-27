# QuikytLoader.AvaloniaUI

UI layer (class library). References Application only - no Infrastructure reference for architectural purity.

## MVVM Pattern with Navigation

The app uses a layered navigation system:

1. **AppViewModel** (App.axaml.cs:27) - Root ViewModel managing navigation
   - Injects HomeViewModel and SettingsViewModel
   - Handles view switching via NavigateToHome/NavigateToSettings commands
   - Maintains selected tab state

2. **HomeViewModel** - Main YouTube download functionality
   - Orchestrates download workflow via DownloadAndSendAsync command
   - Uses IYouTubeDownloadService for downloads
   - Progress reporting via IProgress<double>
   - Status management and validation

3. **SettingsViewModel** - Telegram bot configuration
   - Manages bot token and chat ID via IUserSettings
   - Provides save/load commands for settings persistence

## UI Structure

MainWindow contains:
- 80px vertical sidebar (Firefox/Zen-style navigation)
- Tab buttons for Home and Settings
- ContentControl bound to AppViewModel.CurrentView for dynamic view switching

## Command Pattern

- CommunityToolkit.Mvvm's [RelayCommand] for UI actions
- Command availability controlled via CanExecute predicates
- NotifyCanExecuteChanged() called when conditions change (URL validity, processing state)

## MVVM Communication

- ViewModels never reference Views directly
- Commands expose functionality to UI via data binding
- Observable properties ([ObservableProperty]) auto-generate INotifyPropertyChanged

## Implementation Notes

### Download Queue System
- HomeViewModel maintains ObservableCollection<DownloadQueueItem> for batch downloads
- Queue processes sequentially: downloads next pending item, sends to Telegram, marks completed
- Each queue item tracks its own status (Pending/Downloading/Completed/Failed/Cancelled)
- Per-item progress reporting via IProgress<double> bound to queue item
- Queue processing runs in background (_isQueueProcessing flag prevents duplicate processing)
- Temp files (media + thumbnail) automatically cleaned up after each queue item completes

### Custom Title Editing Workflow
- Two-step process when UseCustomTitle checkbox is checked:
  1. First click "Add to Queue": fetches video title via GetVideoTitleUseCase, populates CustomTitle field
  2. User edits title, clicks "Proceed": adds to queue with custom title
- Button text dynamically changes: "Add to Queue" -> "Proceed"
- IsProceedButtonState flag tracks button state for UI styling
- State resets when: URL changes, UseCustomTitle unchecked, or item added to queue

### File Handling and Cleanup
- YtDlpService uses sanitized video titles as filenames via `%(title)s` template
- Custom titles sanitized via SanitizeFilename (replaces invalid chars with underscore)
- All downloads stored in temp directory: `/tmp/QuikytLoader`
- Files NOT saved to user's Downloads folder - only temporary for Telegram upload
- Thumbnail processing: crops to square, resizes to 320x320 max for Telegram
- HomeViewModel handles cleanup: deletes both media file and thumbnail after sending
- Cleanup happens in finally block to ensure temp files removed even on errors
- DownloadResultEntity contains TempMediaFilePath and TempThumbnailPath properties

### yt-dlp Integration
- All yt-dlp arguments constructed in YtDlpService.BuildAudioDownloadArguments method
- Metadata mapping: YouTube fields -> MP3 ID3 tags
- Progress extracted via regex from stdout/stderr in YtDlpService
- Comprehensive metadata embedding (Artist, Album, Composer, Publisher, etc.)
- Thumbnail embedding with automatic format conversion to JPG
- Process cancellation: kills yt-dlp process tree on CancellationToken

### Download History and Duplicate Detection
- HomeViewModel checks for duplicates before adding to queue using IDownloadHistoryRepository
- Duplicate detection extracts YouTube ID via IYoutubeExtractorService and queries SQLite
- Currently logs duplicate warning to console (UI dialog to be implemented)
- After successful Telegram send, saves record to history with:
  - YouTube video ID (11 chars, primary key)
  - Video title (custom or original from filename)
  - Download timestamp (ISO 8601 UTC format)
- Thumbnail URLs can be derived from YouTube ID when needed (via GetThumbnailUrlAsync)
- Re-downloading same video updates the DownloadedAt timestamp (INSERT OR REPLACE)
- Database: `~/.config/QuikytLoader/history.db`
- Schema: DownloadHistory table (YouTubeId, VideoTitle, DownloadedAt)

## Dependency Injection

`AvaloniaUIServiceExtensions.cs` registers ViewModels as Transient:
- AppViewModel
- HomeViewModel
- SettingsViewModel
