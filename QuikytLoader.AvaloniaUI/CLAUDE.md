# QuikytLoader.AvaloniaUI

UI layer (class library). References Application and Domain only - no Infrastructure reference for architectural purity.

## MVVM Pattern with Navigation

The app uses a layered navigation system:

1. **AppViewModel** - Root ViewModel managing navigation
   - Injects HomeViewModel and SettingsViewModel
   - Handles view switching via NavigateToHome/NavigateToSettings commands
   - Maintains selected tab state

2. **HomeViewModel** - Main Youtube download functionality
   - Orchestrates download workflow via DownloadAndSendAsync command
   - Uses IYoutubeDownloadService for downloads
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

### yt-dlp Integration
- All yt-dlp arguments constructed in YtDlpService.BuildAudioDownloadArguments method
- Metadata mapping: Youtube fields -> MP3 ID3 tags
- Progress extracted via regex from stdout/stderr in YtDlpService
- Comprehensive metadata embedding (Artist, Album, Composer, Publisher, etc.)
- Thumbnail embedding with automatic format conversion to JPG
- Process cancellation: kills yt-dlp process tree on CancellationToken

### Download History and Duplicate Detection
- HomeViewModel checks for duplicates before adding to queue using IDownloadHistoryRepository
- Duplicate detection
- After successful Telegram send, saves record to history with:
  - Youtube video ID (11 chars, primary key)
  - Video title (custom or original from filename)
  - Download timestamp (ISO 8601 UTC format)
- Thumbnail URLs can be derived from Youtube ID when needed (via GetThumbnailUrlAsync)
- Re-downloading same video updates the DownloadedAt timestamp (INSERT OR REPLACE)
- Database: `~/.config/QuikytLoader/history.db`
- Schema: DownloadHistory table (YoutubeVideoId, VideoTitle, DownloadedAt)

## Dependency Injection

`AvaloniaUIServiceExtensions.cs` registers ViewModels and other classes:
- AppViewModel
- HomeViewModel
- SettingsViewModel
