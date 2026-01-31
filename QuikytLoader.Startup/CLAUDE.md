# QuikytLoader.Startup

Entry point and composition root. References Application, Infrastructure, and AvaloniaUI.

## Composition Root

Program.cs composes all DI registrations:

```csharp
services.AddApplicationServices();      // Use cases (Transient)
services.AddInfrastructureServices();   // External services (Singleton)
services.AddAvaloniaUIServices();       // ViewModels (Transient)
```

This is the only project that references Infrastructure, achieving architectural purity where UI layer only depends on Application layer.

## Host Lifecycle

- `host.StartAsync()` called before Avalonia starts
- `StartWithClassicDesktopLifetime()` runs Avalonia event loop (blocking)
- `host.StopAsync()` and `host.Dispose()` called in finally block after Avalonia exits
- This ensures proper disposal of singleton services (e.g., TelegramBotService)

## Entry Point

App.axaml.cs receives IServiceProvider via constructor injection (no IHost). The Startup project creates the host and passes the service provider to App.

Constructor injection is used throughout. Avoid using the service provider directly outside of this composition root.
