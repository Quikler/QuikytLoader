# QuikytLoader.Application

Application layer containing use cases and interface definitions. References Domain only.

## Use Cases

**DownloadAndSendUseCase** - Main workflow orchestration
- Downloads YouTube video as MP3 via IYouTubeDownloadService
- Sends to Telegram via ITelegramBotService
- Saves to history via IDownloadHistoryRepository
- Returns DownloadResultDto

**FindExistingDownloadUseCase** - Duplicate detection
- Extracts YouTube ID from URL via IYoutubeExtractorService
- Checks history via IDownloadHistoryRepository
- Returns DownloadHistoryDto if found

**GetVideoTitleUseCase** - Fetch video title
- Extracts title from YouTube URL via IYtDlpService
- Used for custom title editing workflow

**ValidateYouTubeUrlUseCase** - URL validation
- Creates YouTubeUrl value object
- Returns Result with validation errors

**ManageSettingsUseCase** - Settings management
- Load/save operations via IUserSettings
- Returns UserSettingsDto

## Interfaces

### Services (in `Interfaces/Services/`)

- **IYouTubeDownloadService** - Download orchestration
- **IYtDlpService** - yt-dlp process execution
- **IThumbnailService** - Thumbnail processing
- **ITelegramBotService** - Telegram integration
- **IYoutubeExtractorService** - YouTube ID extraction

### Repositories (in `Interfaces/Repositories/`)

- **IDownloadHistoryRepository** - Download history persistence
- **IDbConnectionFactory** - Database connection management

### Settings (in `Interfaces/Settings/`)

- **IUserSettings** - User settings persistence

## DTOs

- **DownloadResultDto** - Download result for UI consumption
- **DownloadHistoryDto** - History record for UI
- **UserSettingsDto** - Settings transfer object

## Dependency Injection

`ApplicationServiceExtensions.cs` registers use cases as Transient:
- DownloadAndSendUseCase
- FindExistingDownloadUseCase
- GetVideoTitleUseCase
- ValidateYouTubeUrlUseCase
- ManageSettingsUseCase
