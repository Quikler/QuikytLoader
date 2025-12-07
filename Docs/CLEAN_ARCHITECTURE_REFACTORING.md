# Clean Architecture Refactoring Plan

## Overview

This document outlines the plan to refactor QuikytLoader from a monolithic single-project structure to Clean Architecture with proper layer separation.

## Current State

**Single Project Structure:**
```
QuikytLoader/
├── Models/              (mixed: entities, DTOs, UI models)
├── Services/            (mixed: interfaces, implementations)
├── ViewModels/          (UI layer)
├── Views/               (UI layer)
├── App.axaml.cs         (DI registration, startup)
└── Program.cs           (entry point)
```

**Problems:**
- UI layer (ViewModels) directly depends on Infrastructure (service implementations)
- No clear separation between business logic and external concerns
- Domain entities mixed with UI-specific models and DTOs
- Difficult to test business logic in isolation
- Cannot reuse business logic in different presentation layers (CLI, Web API, etc.)
- Infrastructure changes ripple through entire codebase

---

## Target Architecture

### Layer Structure

```
QuikytLoader/
├── QuikytLoader.Domain/              # Layer 1: Core business entities
├── QuikytLoader.Application/         # Layer 2: Business workflows & interfaces
├── QuikytLoader.Infrastructure/      # Layer 3: External services implementation
└── QuikytLoader.AvaloniaUI/          # Layer 4: Avalonia UI (one of many possible presentation layers)
```

### Dependency Rules

**Allowed Dependencies:**
```
AvaloniaUI → Application → Domain
Infrastructure → Application → Domain
CLI (future) → Application → Domain
WebAPI (future) → Application → Domain
```

**Forbidden Dependencies:**
```
Domain → (anything)
Application → Infrastructure
Application → AvaloniaUI (or any other UI layer)
AvaloniaUI → Infrastructure
AvaloniaUI → Domain (except DTOs)
Infrastructure → AvaloniaUI (or any other UI layer)
```

---

## Layer 1: QuikytLoader.Domain

**Project Type:** .NET 9 Class Library
**Dependencies:** NONE
**NuGet Packages:** None (pure .NET)

### Purpose
Contains core business entities, value objects, and domain logic. This layer has zero dependencies on frameworks or external libraries.

### Structure
```
QuikytLoader.Domain/
├── Entities/
│   ├── YouTubeVideo.cs           # Core entity representing a YouTube video
│   ├── DownloadRecord.cs         # Download history domain entity
│   └── TelegramConfig.cs         # Bot configuration entity
├── ValueObjects/
│   ├── YouTubeUrl.cs             # Value object with URL validation logic
│   ├── YouTubeId.cs              # 11-character video ID with validation
│   └── FilePath.cs               # Type-safe file path value object
└── Enums/
    └── DownloadStatus.cs         # Pending, Downloading, Completed, Failed, Cancelled
```

### File Mapping

| Current File | New Location | Transformation |
|-------------|--------------|----------------|
| `Models/DownloadStatus.cs` | `Domain/Enums/DownloadStatus.cs` | Move as-is |
| `Models/DownloadHistoryRecord.cs` | `Domain/Entities/DownloadRecord.cs` | Rename, remove data annotations |
| N/A | `Domain/Entities/YouTubeVideo.cs` | New entity |
| N/A | `Domain/ValueObjects/YouTubeUrl.cs` | New value object |
| N/A | `Domain/ValueObjects/YouTubeId.cs` | New value object |

### Example: YouTubeVideo Entity

```csharp
namespace QuikytLoader.Domain.Entities;

/// <summary>
/// Domain entity representing a YouTube video
/// </summary>
public class YouTubeVideo
{
    public required YouTubeId Id { get; init; }
    public required string Title { get; init; }
    public string? ThumbnailUrl { get; init; }

    // Domain logic
    public string GetSanitizedTitle()
    {
        return string.Join("_", Title.Split(Path.GetInvalidFileNameChars()));
    }
}
```

### Example: YouTubeId Value Object

```csharp
namespace QuikytLoader.Domain.ValueObjects;

/// <summary>
/// Value object representing a YouTube video ID (always 11 characters)
/// </summary>
public record YouTubeId
{
    private const int ValidLength = 11;

    public string Value { get; }

    public YouTubeId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("YouTube ID cannot be empty");

        if (value.Length != ValidLength)
            throw new ArgumentException($"YouTube ID must be exactly {ValidLength} characters");

        Value = value;
    }

    public static implicit operator string(YouTubeId id) => id.Value;
    public override string ToString() => Value;
}
```

---

## Layer 2: QuikytLoader.Application

**Project Type:** .NET 9 Class Library
**Dependencies:** QuikytLoader.Domain
**NuGet Packages:**
- `CommunityToolkit.Mvvm` (for observable properties in DTOs if needed)

### Purpose
Contains business workflows (use cases), application services, DTOs, and interfaces for infrastructure. This is where business logic orchestration happens.

### Structure
```
QuikytLoader.Application/
├── UseCases/
│   ├── DownloadAndSendUseCase.cs       # Orchestrates: download → history → telegram
│   ├── CheckDuplicateUseCase.cs        # Check if video already downloaded
│   ├── GetVideoInfoUseCase.cs          # Fetch video title without downloading
│   └── ManageSettingsUseCase.cs        # Load/save settings
├── DTOs/
│   ├── DownloadResultDto.cs            # Result of download operation
│   ├── DownloadHistoryRecordDto.cs     # History record for UI binding
│   └── AppSettingsDto.cs               # Settings data transfer object
├── Interfaces/
│   ├── Services/
│   │   ├── IYouTubeDownloadService.cs  # YouTube download abstraction
│   │   ├── ITelegramBotService.cs      # Telegram send abstraction
│   │   └── IYoutubeExtractorService.cs        # URL → ID extraction
│   └── Repositories/
│       ├── IDownloadHistoryRepository.cs  # Download history persistence
│       └── ISettingsRepository.cs         # Settings persistence
├── Mappers/
│   ├── DownloadMapper.cs               # Entity ↔ DTO mapping
│   └── SettingsMapper.cs               # Entity ↔ DTO mapping
└── DependencyInjection/
    └── ApplicationServiceExtensions.cs  # services.AddApplicationServices()
```

### File Mapping

| Current File | New Location | Transformation |
|-------------|--------------|----------------|
| `Services/IYouTubeDownloadService.cs` | `Application/Interfaces/Services/IYouTubeDownloadService.cs` | Move, update return types to DTOs |
| `Services/ITelegramBotService.cs` | `Application/Interfaces/Services/ITelegramBotService.cs` | Move as-is |
| `Services/IYoutubeExtractorService.cs` | `Application/Interfaces/Services/IYoutubeExtractorService.cs` | Move, return YouTubeId value object |
| `Services/IDownloadHistoryService.cs` | `Application/Interfaces/Repositories/IDownloadHistoryRepository.cs` | Rename, refactor to repository pattern |
| `Services/ISettingsManager.cs` | `Application/Interfaces/Repositories/ISettingsRepository.cs` | Rename to repository pattern |
| `Services/IDbConnectionService.cs` | `Application/Interfaces/Repositories/IDbConnectionFactory.cs` | Rename for clarity |
| `Models/DownloadResult.cs` | `Application/DTOs/DownloadResultDto.cs` | Rename to DTO |
| `Models/AppSettings.cs` | `Application/DTOs/AppSettingsDto.cs` | Rename to DTO |

### Example: DownloadAndSendUseCase

```csharp
namespace QuikytLoader.Application.UseCases;

/// <summary>
/// Use case: Download YouTube video, save to history, send to Telegram
/// Orchestrates multiple services to complete the workflow
/// </summary>
public class DownloadAndSendUseCase
{
    private readonly IYouTubeDownloadService _downloadService;
    private readonly IDownloadHistoryRepository _historyRepo;
    private readonly ITelegramBotService _telegramService;
    private readonly IYoutubeExtractorService _youtubeExtractorService;

    public DownloadAndSendUseCase(
        IYouTubeDownloadService downloadService,
        IDownloadHistoryRepository historyRepo,
        ITelegramBotService telegramService,
        IYoutubeExtractor youtubeExtractorService)
    {
        _downloadService = downloadService;
        _historyRepo = historyRepo;
        _telegramService = telegramService;
        _youtubeExtractorService = youtubeExtractorService;
    }

    public async Task<DownloadResultDto> ExecuteAsync(
        string url,
        string? customTitle = null,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Extract YouTube ID
        var youtubeId = await _youtubeExtractorService.ExtractVideoIdAsync(url);

        // 2. Download video
        var result = customTitle != null
            ? await _downloadService.DownloadAsync(url, customTitle, progress, cancellationToken)
            : await _downloadService.DownloadAsync(url, progress, cancellationToken);

        // 3. Send to Telegram
        await _telegramService.SendAudioAsync(
            result.TempMediaFilePath,
            result.TempThumbnailPath);

        // 4. Save to history
        var record = new DownloadRecord
        {
            YouTubeId = youtubeId,
            VideoTitle = customTitle ?? Path.GetFileNameWithoutExtension(result.TempMediaFilePath),
            DownloadedAt = DateTime.UtcNow.ToString("o")
        };
        await _historyRepo.SaveAsync(record);

        // 5. Return DTO
        return DownloadMapper.ToDto(result);
    }
}
```

### Example: IDownloadHistoryRepository

```csharp
namespace QuikytLoader.Application.Interfaces.Repositories;

/// <summary>
/// Repository pattern for download history persistence
/// </summary>
public interface IDownloadHistoryRepository
{
    Task SaveAsync(DownloadRecord record);
    Task<DownloadRecord?> GetByIdAsync(YouTubeId id);
    Task<IEnumerable<DownloadRecord>> GetAllAsync();
}
```

---

## Layer 3: QuikytLoader.Infrastructure

**Project Type:** .NET 9 Class Library
**Dependencies:** QuikytLoader.Application, QuikytLoader.Domain
**NuGet Packages:**
- `Microsoft.Data.Sqlite`
- `Dapper`
- `Telegram.Bot`
- `SixLabors.ImageSharp`

### Purpose
Implements all external service interfaces defined in Application layer. Contains all third-party dependencies.

### Structure
```
QuikytLoader.Infrastructure/
├── YouTube/
│   ├── YouTubeDownloadService.cs      # yt-dlp integration
│   └── YoutubeExtractor.cs            # URL parsing & yt-dlp ID extraction
├── Telegram/
│   └── TelegramBotService.cs          # Telegram.Bot API integration
├── Persistence/
│   ├── Repositories/
│   │   ├── DownloadHistoryRepository.cs  # SQLite + Dapper
│   │   └── SettingsRepository.cs         # JSON file persistence
│   └── DbConnectionFactory.cs            # SQLite connection management
└── DependencyInjection/
    └── InfrastructureServiceExtensions.cs  # services.AddInfrastructureServices()
```

### File Mapping

| Current File | New Location | Transformation |
|-------------|--------------|----------------|
| `Services/YouTubeDownloadService.cs` | `Infrastructure/YouTube/YouTubeDownloadService.cs` | Move, implement interface from Application |
| `Services/YoutubeExtractor.cs` | `Infrastructure/YouTube/YoutubeExtractor.cs` | Move, return YouTubeId value object |
| `Services/TelegramBotService.cs` | `Infrastructure/Telegram/TelegramBotService.cs` | Move as-is |
| `Services/DownloadHistoryService.cs` | `Infrastructure/Persistence/Repositories/DownloadHistoryRepository.cs` | Rename to repository pattern |
| `Services/SettingsManager.cs` | `Infrastructure/Persistence/Repositories/SettingsRepository.cs` | Rename to repository pattern |
| `Services/DbConnectionService.cs` | `Infrastructure/Persistence/DbConnectionFactory.cs` | Rename for clarity |

### Example: InfrastructureServiceExtensions

```csharp
namespace QuikytLoader.Infrastructure.DependencyInjection;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // YouTube services
        services.AddSingleton<IYouTubeDownloadService, YouTubeDownloadService>();
        services.AddSingleton<IYoutubeExtractor, YoutubeExtractor>();

        // Telegram services
        services.AddSingleton<ITelegramBotService, TelegramBotService>();

        // Persistence
        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
        services.AddSingleton<IDownloadHistoryRepository, DownloadHistoryRepository>();
        services.AddSingleton<ISettingsRepository, SettingsRepository>();

        return services;
    }
}
```

---

## Layer 4: QuikytLoader.AvaloniaUI

**Project Type:** Avalonia Application (.NET 9)
**Dependencies:** QuikytLoader.Application
**NuGet Packages:**
- All Avalonia packages
- `FluentAvaloniaUI`
- `CommunityToolkit.Mvvm`
- `Microsoft.Extensions.Hosting`
- `Microsoft.Extensions.DependencyInjection`

### Purpose
Avalonia UI presentation layer. Contains ViewModels, Views, and UI-specific models. Depends ONLY on Application layer. This is just one possible presentation layer - future layers like CLI or WebAPI would have the same dependency structure.

### Structure
```
QuikytLoader.AvaloniaUI/
├── ViewModels/
│   ├── AppViewModel.cs              # Root navigation ViewModel
│   ├── HomeViewModel.cs             # YouTube download UI (uses Use Cases)
│   ├── SettingsViewModel.cs         # Settings UI (uses Use Cases)
│   ├── MainWindowViewModel.cs       # Main window ViewModel
│   └── ViewModelBase.cs             # Base class
├── Views/
│   ├── MainWindow.axaml/cs          # Main window with sidebar
│   ├── HomeView.axaml/cs            # Download queue UI
│   └── SettingsView.axaml/cs        # Settings form
├── Models/
│   └── DownloadQueueItem.cs         # UI-specific observable model
├── Assets/
├── App.axaml.cs                     # DI registration, startup
└── Program.cs                       # Entry point
```

### File Mapping

| Current File | New Location | Transformation |
|-------------|--------------|----------------|
| `ViewModels/**` | `AvaloniaUI/ViewModels/**` | Refactor to use Use Cases instead of services |
| `Views/**` | `AvaloniaUI/Views/**` | Move as-is |
| `Models/DownloadQueueItem.cs` | `AvaloniaUI/Models/DownloadQueueItem.cs` | Move as-is (UI-specific) |
| `App.axaml.cs` | `AvaloniaUI/App.axaml.cs` | Update DI registration |
| `Program.cs` | `AvaloniaUI/Program.cs` | Move as-is |

### Example: Updated HomeViewModel

**Before (directly uses services):**
```csharp
public partial class HomeViewModel(
    IYouTubeDownloadService youtubeService,
    ITelegramBotService telegramService,
    IDownloadHistoryService historyService,
    IYoutubeExtractor youtubeExtractorService) : ViewModelBase
{
    [RelayCommand]
    private async Task DownloadAndSendAsync()
    {
        var result = await youtubeService.DownloadAsync(YoutubeUrl, progress);
        await historyService.SaveDownloadAsync(result.YouTubeId, title);
        await telegramService.SendAudioAsync(result.TempMediaFilePath);
    }
}
```

**After (uses Use Case):**
```csharp
public partial class HomeViewModel(
    DownloadAndSendUseCase downloadAndSendUseCase,
    CheckDuplicateUseCase checkDuplicateUseCase) : ViewModelBase
{
    [RelayCommand]
    private async Task DownloadAndSendAsync()
    {
        // Check for duplicate
        if (await checkDuplicateUseCase.GetExistingRecordAsync(YoutubeUrl) is not null)
        {
            // Show warning to user
        }

        // Execute workflow
        var result = await downloadAndSendUseCase.ExecuteAsync(
            YoutubeUrl,
            CustomTitle,
            progress,
            cancellationToken);
    }
}
```

### Example: Updated App.axaml.cs

```csharp
public partial class App : Application
{
    private IHost? _host;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Register layers
                    services.AddApplicationServices();      // Use Cases, DTOs
                    services.AddInfrastructureServices();    // Service implementations

                    // Register ViewModels
                    services.AddTransient<AppViewModel>();
                    services.AddTransient<HomeViewModel>();
                    services.AddTransient<SettingsViewModel>();
                    services.AddTransient<MainWindowViewModel>();
                })
                .Build();

            var appViewModel = _host.Services.GetRequiredService<AppViewModel>();
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(appViewModel)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
```

---

## Migration Steps

### Phase 1: Create Project Structure

```bash
# Navigate to QuikytLoader directory
cd /home/quikler/Desktop/repos/QuikytLoader

# Create new class library projects
dotnet new classlib -n QuikytLoader.Domain -f net9.0
dotnet new classlib -n QuikytLoader.Application -f net9.0
dotnet new classlib -n QuikytLoader.Infrastructure -f net9.0

# Create solution file
dotnet new sln -n QuikytLoader

# Add all projects to solution
dotnet sln add QuikytLoader.Domain/QuikytLoader.Domain.csproj
dotnet sln add QuikytLoader.Application/QuikytLoader.Application.csproj
dotnet sln add QuikytLoader.Infrastructure/QuikytLoader.Infrastructure.csproj
dotnet sln add QuikytLoader.csproj  # Rename to AvaloniaUI later

# Add project references (enforce dependency rules)
cd QuikytLoader.Application
dotnet add reference ../QuikytLoader.Domain/QuikytLoader.Domain.csproj

cd ../QuikytLoader.Infrastructure
dotnet add reference ../QuikytLoader.Application/QuikytLoader.Application.csproj
dotnet add reference ../QuikytLoader.Domain/QuikytLoader.Domain.csproj

cd ../QuikytLoader  # AvaloniaUI project
dotnet add reference ../QuikytLoader.Application/QuikytLoader.Application.csproj
```

### Phase 2: Move Domain Layer (No Dependencies)

1. Create `Domain/Entities/`, `Domain/ValueObjects/`, `Domain/Enums/`
2. Move `Models/DownloadStatus.cs` → `Domain/Enums/DownloadStatus.cs`
3. Create domain entities and value objects
4. Build and verify: `dotnet build QuikytLoader.Domain`

### Phase 3: Move Application Layer (Depends on Domain)

1. Create folder structure in Application project
2. Move all `Services/I*.cs` interfaces to `Application/Interfaces/`
3. Rename interfaces to match new patterns (Repository, Factory)
4. Create DTOs from existing models
5. Create Use Case classes
6. Create mapper classes
7. Add NuGet packages: `CommunityToolkit.Mvvm`
8. Build and verify: `dotnet build QuikytLoader.Application`

### Phase 4: Move Infrastructure Layer

1. Create folder structure in Infrastructure project
2. Move service implementations to appropriate folders
3. Update implementations to use Application interfaces
4. Update implementations to use Domain entities
5. Create `InfrastructureServiceExtensions.cs`
6. Add NuGet packages: `Microsoft.Data.Sqlite`, `Dapper`, `Telegram.Bot`, `SixLabors.ImageSharp`
7. Build and verify: `dotnet build QuikytLoader.Infrastructure`

### Phase 5: Update AvaloniaUI Layer

1. Refactor ViewModels to inject Use Cases instead of services
2. Update ViewModels to work with DTOs
3. Update `App.axaml.cs` DI registration
4. Update namespaces throughout
5. Keep UI-specific models (DownloadQueueItem)
6. Build and verify: `dotnet build QuikytLoader`

### Phase 6: Final Integration & Testing

1. Run application: `dotnet run --project QuikytLoader`
2. Test all functionality: download, send, history, settings
3. Verify proper cleanup of temp files
4. Update CLAUDE.md with new architecture
5. Delete old files and clean up

---

## File Movement Checklist

### Domain Layer Files

- [ ] `Models/DownloadStatus.cs` → `Domain/Enums/DownloadStatus.cs`
- [ ] Create `Domain/Entities/YouTubeVideo.cs`
- [ ] Create `Domain/Entities/DownloadRecord.cs`
- [ ] Create `Domain/Entities/TelegramConfig.cs`
- [ ] Create `Domain/ValueObjects/YouTubeId.cs`
- [ ] Create `Domain/ValueObjects/YouTubeUrl.cs`

### Application Layer Files

- [ ] `Services/IYouTubeDownloadService.cs` → `Application/Interfaces/Services/IYouTubeDownloadService.cs`
- [ ] `Services/ITelegramBotService.cs` → `Application/Interfaces/Services/ITelegramBotService.cs`
- [ ] `Services/IYoutubeExtractor.cs` → `Application/Interfaces/Services/IYoutubeExtractor.cs`
- [ ] `Services/IDownloadHistoryService.cs` → `Application/Interfaces/Repositories/IDownloadHistoryRepository.cs`
- [ ] `Services/ISettingsManager.cs` → `Application/Interfaces/Repositories/ISettingsRepository.cs`
- [ ] `Services/IDbConnectionService.cs` → `Application/Interfaces/Repositories/IDbConnectionFactory.cs`
- [ ] `Models/DownloadResult.cs` → `Application/DTOs/DownloadResultDto.cs`
- [ ] `Models/AppSettings.cs` → `Application/DTOs/AppSettingsDto.cs`
- [ ] `Models/DownloadHistoryRecord.cs` → `Application/DTOs/DownloadHistoryRecordDto.cs`
- [ ] Create `Application/UseCases/DownloadAndSendUseCase.cs`
- [ ] Create `Application/UseCases/CheckDuplicateUseCase.cs`
- [ ] Create `Application/UseCases/GetVideoInfoUseCase.cs`
- [ ] Create `Application/UseCases/ManageSettingsUseCase.cs`
- [ ] Create `Application/Mappers/DownloadMapper.cs`
- [ ] Create `Application/DependencyInjection/ApplicationServiceExtensions.cs`

### Infrastructure Layer Files

- [ ] `Services/YouTubeDownloadService.cs` → `Infrastructure/YouTube/YouTubeDownloadService.cs`
- [ ] `Services/YoutubeExtractor.cs` → `Infrastructure/YouTube/YoutubeExtractor.cs`
- [ ] `Services/TelegramBotService.cs` → `Infrastructure/Telegram/TelegramBotService.cs`
- [ ] `Services/DownloadHistoryService.cs` → `Infrastructure/Persistence/Repositories/DownloadHistoryRepository.cs`
- [ ] `Services/SettingsManager.cs` → `Infrastructure/Persistence/Repositories/SettingsRepository.cs`
- [ ] `Services/DbConnectionService.cs` → `Infrastructure/Persistence/DbConnectionFactory.cs`
- [ ] Create `Infrastructure/DependencyInjection/InfrastructureServiceExtensions.cs`

### AvaloniaUI Layer Files (Stay in root project)

- [ ] `ViewModels/AppViewModel.cs` (refactor to use Use Cases)
- [ ] `ViewModels/HomeViewModel.cs` (refactor to use Use Cases)
- [ ] `ViewModels/SettingsViewModel.cs` (refactor to use Use Cases)
- [ ] `ViewModels/MainWindowViewModel.cs` (no changes)
- [ ] `ViewModels/ViewModelBase.cs` (no changes)
- [ ] `Views/**` (no changes)
- [ ] `Models/DownloadQueueItem.cs` (stays as UI-specific model)
- [ ] `App.axaml.cs` (update DI registration)
- [ ] `Program.cs` (no changes)

---

## Dependency Validation

After migration, verify these rules are enforced:

### Domain Project
```bash
# Should have ZERO dependencies
dotnet list QuikytLoader.Domain package
# Expected: No packages
```

### Application Project
```bash
# Should only reference Domain
dotnet list QuikytLoader.Application reference
# Expected: ../QuikytLoader.Domain/QuikytLoader.Domain.csproj
```

### Infrastructure Project
```bash
# Should reference Application + Domain
dotnet list QuikytLoader.Infrastructure reference
# Expected:
#   ../QuikytLoader.Application/QuikytLoader.Application.csproj
#   ../QuikytLoader.Domain/QuikytLoader.Domain.csproj
```

### AvaloniaUI Project
```bash
# Should ONLY reference Application (NOT Infrastructure or Domain)
dotnet list QuikytLoader reference
# Expected: ../QuikytLoader.Application/QuikytLoader.Application.csproj
```

---

## Benefits Summary

### Before (Monolithic)
- UI directly depends on Infrastructure services
- Cannot test business logic without external dependencies
- Difficult to change implementations
- Cannot reuse logic in other UIs
- Circular dependency risks

### After (Clean Architecture)
- Clear layer separation with enforced dependency rules
- Business logic (Domain + Application) has no infrastructure dependencies
- Easy to test: mock interfaces in Application layer
- Can swap implementations (SQLite → PostgreSQL, yt-dlp → youtube-dl)
- Can reuse Application layer in CLI, Web API, or mobile apps
- Domain layer is pure business logic with zero framework coupling

---

## Testing Strategy

### Domain Layer Tests
```csharp
// Pure unit tests, no mocking needed
[Fact]
public void YouTubeId_ThrowsException_WhenInvalidLength()
{
    Assert.Throws<ArgumentException>(() => new YouTubeId("short"));
}
```

### Application Layer Tests
```csharp
// Mock infrastructure interfaces
[Fact]
public async Task DownloadAndSendUseCase_SavesHistory_AfterSuccessfulDownload()
{
    var mockDownloadService = new Mock<IYouTubeDownloadService>();
    var mockHistoryRepo = new Mock<IDownloadHistoryRepository>();
    var mockTelegramService = new Mock<ITelegramBotService>();

    var useCase = new DownloadAndSendUseCase(
        mockDownloadService.Object,
        mockHistoryRepo.Object,
        mockTelegramService.Object);

    await useCase.ExecuteAsync("https://youtube.com/watch?v=test");

    mockHistoryRepo.Verify(x => x.SaveAsync(It.IsAny<DownloadRecord>()), Times.Once);
}
```

### Infrastructure Layer Tests
```csharp
// Integration tests with real dependencies
[Fact]
public async Task DownloadHistoryRepository_SavesAndRetrievesRecord()
{
    var factory = new DbConnectionFactory();
    var repo = new DownloadHistoryRepository(factory);

    var record = new DownloadRecord
    {
        YouTubeId = new YouTubeId("testid12345"),
        VideoTitle = "Test Video",
        DownloadedAt = DateTime.UtcNow.ToString("o")
    };

    await repo.SaveAsync(record);
    var retrieved = await repo.GetByIdAsync(new YouTubeId("testid12345"));

    Assert.NotNull(retrieved);
    Assert.Equal("Test Video", retrieved.VideoTitle);
}
```

---

## Notes

- **Backward Compatibility**: This is a structural refactoring. All functionality remains the same.
- **Git History**: Consider creating a new branch `feature/clean-architecture` for this migration.
- **Incremental Migration**: Each phase can be committed separately for easier rollback.
- **Performance**: No performance impact expected. Dependency injection overhead is negligible.
- **File Locations**: After migration, the main `.csproj` should be renamed to `QuikytLoader.AvaloniaUI.csproj`.
- **Future Presentation Layers**: The architecture supports adding additional presentation layers (CLI, WebAPI, Mobile) that would all depend on the same Application layer, sharing business logic and use cases.

---

## References

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [.NET Clean Architecture Template](https://github.com/jasontaylordev/CleanArchitecture)
- [Dependency Inversion Principle](https://en.wikipedia.org/wiki/Dependency_inversion_principle)
