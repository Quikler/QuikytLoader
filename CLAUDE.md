# CLAUDE.md

This file provides guidance to Claude Code when working with code in this repository.

## Project Overview

QuikytLoader is a .NET Avalonia UI desktop application for downloading Youtube videos as MP3 files and sending them to Telegram. Follows MVVM architecture with clean separation of concerns.

## Build and Run Commands

```bash
# Build the project
dotnet build

# Run the application (from Startup project)
make run

# Build for release (trimmed)
make trimmed

# Build for release (NativeAOT)
make aot

# Go to output location after publish:
. output.sh
```

## External Dependencies

- **yt-dlp**: Must be installed on system (install via package manager, e.g. `sudo pacman -S yt-dlp` on Arch)

## Architecture Overview

### Clean Architecture with Separate Composition Root

The solution follows strict Clean Architecture with a dedicated Startup project as the composition root:

```
QuikytLoader.Startup (exe)        <- Entry point & DI composition
├── References: Application, Infrastructure, AvaloniaUI
└── Program.cs: ServiceCollection with all DI registrations

QuikytLoader.AvaloniaUI (library) <- UI layer (class library)
├── References: Application only  <- No Infrastructure reference
└── App.axaml.cs: Receives IServiceProvider via constructor injection

QuikytLoader.Application          <- Use cases and interfaces
├── References: Domain only
└── DependencyInjection/ApplicationServiceExtensions.cs

QuikytLoader.Infrastructure       <- External service implementations
├── References: Application, Domain
└── DependencyInjection/InfrastructureServiceExtensions.cs

QuikytLoader.Domain               <- Core entities and value objects
└── No external dependencies
```

This achieves architectural purity: UI layer only depends on Domain and Application layers, with Infrastructure reference isolated to the Startup composition root.

## Key Design Patterns

- **Single Responsibility**: Each method has one clear purpose (e.g., ValidateUrl, UpdateProgress, CleanupTempFiles)
- **Law of Demeter**: No deep property chains, clear interfaces between layers

## Per-Project Documentation

Each project has its own CLAUDE.md with layer-specific guidance:

- **[QuikytLoader.Domain/CLAUDE.md](QuikytLoader.Domain/CLAUDE.md)** - Entities, value objects, Result pattern
- **[QuikytLoader.Application/CLAUDE.md](QuikytLoader.Application/CLAUDE.md)** - Use cases, interface definitions, DTOs
- **[QuikytLoader.Infrastructure/CLAUDE.md](QuikytLoader.Infrastructure/CLAUDE.md)** - Service implementations, settings, security
- **[QuikytLoader.AvaloniaUI/CLAUDE.md](QuikytLoader.AvaloniaUI/CLAUDE.md)** - ViewModels, MVVM patterns, UI structure, implementation notes
- **[QuikytLoader.Startup/CLAUDE.md](QuikytLoader.Startup/CLAUDE.md)** - DI composition, entry point

## Anti-Patterns

See [.claude/anti-patterns.md](.claude/anti-patterns.md) for common mistakes to avoid in this codebase. Includes architecture violations, DI misuse, MVVM anti-patterns, Result pattern misuse, and general C# anti-patterns with code examples.
