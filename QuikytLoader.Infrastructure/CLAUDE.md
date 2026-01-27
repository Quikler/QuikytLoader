# QuikytLoader.Infrastructure

External service implementations. References Application and Domain layers.

## Services

**YouTubeDownloadService** - Download orchestration
- Coordinates download workflow by delegating to specialized services
- Downloads to system temp directory (`/tmp/QuikytLoader`) only
- Files not saved to user's Downloads - temporary for Telegram upload
- Delegates yt-dlp execution to IYtDlpService
- Delegates thumbnail processing to IThumbnailService
- Uses IYoutubeExtractorService to extract video IDs
- Returns DownloadResultEntity with YouTubeId, TempMediaFilePath, TempThumbnailPath

**YtDlpService** - yt-dlp process wrapper
- Executes yt-dlp process for audio downloads and metadata extraction
- Builds command arguments via `BuildAudioDownloadArguments` method
- Embeds comprehensive metadata (Artist, Album, Composer, Performer, Publisher, etc.)
- Embeds video thumbnail as album art with automatic format conversion to JPG
- Parses progress from yt-dlp output via regex
- Process cancellation: kills yt-dlp process tree on CancellationToken
- Auto-normalizes filenames (removes extra whitespace)

**ThumbnailService** - Thumbnail processing
- Processes thumbnails for Telegram requirements
- Crops images to square aspect ratio
- Resizes to 320x320 max dimensions for Telegram

**YoutubeExtractorService** - YouTube ID extraction
- Fast regex-based extraction for common URL formats (youtube.com/watch?v=ID, youtu.be/ID)
- Fallback to yt-dlp `--print id` for edge cases
- Returns 11-character YouTube video ID

**TelegramBotService** - Telegram integration
- Lazy initialization: bot client created on first SendAudioAsync call
- Reloads settings on each send to pick up configuration changes
- Sends MP3 files with optional thumbnail to configured chat ID
- Uses Telegram.Bot library (v22.7.5)
- Implements IDisposable for proper cleanup on app shutdown

**DownloadHistoryRepository** - Download history tracking
- Stores YouTube download history in SQLite database
- Checks for duplicate downloads by YouTube ID
- Saves download records with video title and timestamp
- Uses INSERT OR REPLACE for upserts (updates DownloadedAt on re-downloads)
- Provides GetThumbnailUrlAsync (derives from YouTube ID via yt-dlp or CDN fallback)

**DbConnectionFactory** - Database connection management
- Manages SQLite at `~/.config/QuikytLoader/history.db`
- SQLite creates database file on first connection
- Initializes schema with CREATE TABLE IF NOT EXISTS (idempotent)
- Sets restrictive file permissions on Linux (mode 600)

**UserSettings** - JSON-based settings persistence
- Stores config in `~/.config/QuikytLoader/settings.json` (XDG Base Directory spec)
- Atomic writes via temp file + rename to prevent corruption
- Sets restrictive file permissions on Linux (mode 600)
- Auto-creates default settings if file doesn't exist or is corrupted

## Settings and Security

- Settings: `~/.config/QuikytLoader/settings.json`
- Database: `~/.config/QuikytLoader/history.db`
- File permissions: mode 600 on Linux (user read/write only)
- TelegramBotService validates settings on each send (throws if BotToken or ChatId missing)
- Bot token: obtain from @BotFather on Telegram
- Chat ID: obtain from @userinfobot on Telegram

## Dependency Injection

`InfrastructureServiceExtensions.cs` registers services as Singleton:
- IUserSettings -> UserSettings
- IYouTubeDownloadService -> YouTubeDownloadService
- IYtDlpService -> YtDlpService
- IThumbnailService -> ThumbnailService
- ITelegramBotService -> TelegramBotService
- IYoutubeExtractorService -> YoutubeExtractorService
- IDbConnectionFactory -> DbConnectionFactory
- IDownloadHistoryRepository -> DownloadHistoryRepository
