# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

QuikytLoader is an Avalonia UI desktop application for downloading YouTube videos as MP3 files and sending them to Telegram. Built for .NET 9 targeting Linux (Arch), following MVVM architecture with clean separation of concerns.

## Build and Run Commands

```bash
# Build the project
dotnet build

# Run the application (from Startup project)
dotnet run --project QuikytLoader.Startup

# Build for release (self-contained)
dotnet publish QuikytLoader.Startup -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true

# Output location after publish:
# QuikytLoader.Startup/bin/Release/net9.0/linux-x64/publish/QuikytLoader.Startup
```

## External Dependencies

- **yt-dlp**: Must be installed on system (`sudo pacman -S yt-dlp` on Arch)
- Used by YtDlpService to download and convert YouTube videos to MP3

## Architecture Overview

### Clean Architecture with Separate Composition Root

The solution follows strict Clean Architecture with a dedicated Startup project as the composition root:

```
QuikytLoader.Startup (exe)        <- Entry point & DI composition
├── References: Application, Infrastructure, AvaloniaUI
└── Program.cs: CreateHostBuilder with all DI registrations

QuikytLoader.AvaloniaUI (library) <- UI layer (class library)
├── References: Application only  <- No Infrastructure reference
└── App.axaml.cs: Receives IServiceProvider via constructor injection (no IHost)

QuikytLoader.Application          <- Use cases and interfaces
├── References: Domain only
└── DependencyInjection/ApplicationServiceExtensions.cs

QuikytLoader.Infrastructure       <- External service implementations
├── References: Application, Domain
└── DependencyInjection/InfrastructureServiceExtensions.cs

QuikytLoader.Domain               <- Core entities and value objects
└── No external dependencies
```

This achieves architectural purity: UI layer only depends on Application layer, with Infrastructure reference isolated to the Startup composition root.

## Key Design Patterns

- **Single Responsibility**: Each method has one clear purpose (e.g., ValidateUrl, UpdateProgress, CleanupTempFiles)
- **Law of Demeter**: No deep property chains, clear interfaces between layers

## Per-Project Documentation

Each project has its own CLAUDE.md with layer-specific guidance:

- **[QuikytLoader.Domain/CLAUDE.md](QuikytLoader.Domain/CLAUDE.md)** - Entities, value objects, Result pattern
- **[QuikytLoader.Application/CLAUDE.md](QuikytLoader.Application/CLAUDE.md)** - Use cases, interface definitions, DTOs
- **[QuikytLoader.Infrastructure/CLAUDE.md](QuikytLoader.Infrastructure/CLAUDE.md)** - Service implementations, settings, security
- **[QuikytLoader.AvaloniaUI/CLAUDE.md](QuikytLoader.AvaloniaUI/CLAUDE.md)** - ViewModels, MVVM patterns, UI structure, implementation notes
- **[QuikytLoader.Startup/CLAUDE.md](QuikytLoader.Startup/CLAUDE.md)** - DI composition, host lifecycle
