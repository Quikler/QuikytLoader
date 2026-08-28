# QuikytLoader.Infrastructure

External service implementations. References Application and Domain layers.

## Services

**YoutubeDownloadService** - Download orchestration
- Coordinates download workflow by delegating to specialized services
- Downloads to system temp directory (`/tmp/QuikytLoader`) only
- Files not saved to user's Downloads - temporary for Telegram upload
- Delegates yt-dlp execution to IYtDlpService
- Delegates thumbnail processing to IThumbnailService
- Returns DownloadResultEntity with YoutubeVideoId, TempMediaFilePath, TempThumbnailPath

**YtDlpService** - yt-dlp process wrapper
- Executes yt-dlp process for audio downloads and metadata extraction
- Builds command arguments via `YtDlpAcl` class
- Embeds comprehensive metadata (Artist, Album, Composer, Performer, Publisher, etc.)
- Embeds video thumbnail as album art with automatic format conversion to JPG
- Parses progress from yt-dlp output via regex
- Process cancellation: kills yt-dlp process tree on CancellationToken
- Auto-normalizes filenames (removes extra whitespace)

**ThumbnailService** - Thumbnail processing
- Processes thumbnails for Telegram requirements
- Crops images to square aspect ratio
- Resizes to 320x320 max dimensions for Telegram

**YoutubeVideoIdParser** - Extract single video ID from URL
- Validates URLs against host allowlist (youtube.com, youtu.be, etc.)
- Multi-pattern regex matching for watch?v=, /v/, /embed/, youtu.be/, etc.
- Validates extracted ID is exactly 11 characters
- Returns Result<string> with video ID or error

**YoutubePlaylistIdParser** - Extract playlist ID from URL
- Extracts `list=` query parameter from URL
- Validates playlist ID format
- Returns Result<string> with playlist ID or error

**TelegramBotService** - Telegram integration
- Lazy initialization: bot client created on first SendAudioAsync call
- Reloads settings on each send to pick up configuration changes
- Sends MP3 files with optional thumbnail to configured chat ID
- Uses Telegram.Bot library (v22.7.5)
- Implements IDisposable for proper cleanup on app shutdown

**DownloadHistoryRepository** - Download history tracking
- Stores Youtube download history in SQLite database
- Checks for duplicate downloads by Youtube ID
- Saves download records with video title and timestamp
- Uses INSERT OR REPLACE for upserts (updates DownloadedAt on re-downloads)
- Provides GetThumbnailUrlAsync (derives from Youtube ID via yt-dlp or CDN fallback)

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
