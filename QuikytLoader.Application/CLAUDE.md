# QuikytLoader.Application

Application layer containing use cases and interface definitions. References Domain only.

## Use Cases

**DownloadAndSendUseCase** - Main workflow orchestration
- Downloads Youtube video as MP3 via IYoutubeDownloadService
- Sends to Telegram via ITelegramBotService
- Saves to history via IDownloadHistoryRepository
- Returns DownloadResultDto

**FindExistingDownloadUseCase** - Duplicate detection
- Extracts Youtube ID from URL via IYoutubeVideoIdParser
- Checks history via IDownloadHistoryRepository
- Returns DownloadHistoryDto if found

**GetVideoTitleUseCase** - Fetch video title
- Extracts title from Youtube URL via IYtDlpService
- Used for custom title editing workflow

**ManageSettingsUseCase** - Settings management
- Load/save operations via IUserSettings
- Returns UserSettingsDto

## Interfaces

### Services (in `Interfaces/Services/`)

- **IYoutubeDownloadService** - Download orchestration
- **IYtDlpService** - yt-dlp process execution
- **IThumbnailService** - Thumbnail processing
- **ITelegramBotService** - Telegram integration

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
- ManageSettingsUseCase
