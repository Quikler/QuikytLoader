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

## Service Lifetime

- `ServiceCollection` builds a `ServiceProvider` before Avalonia starts
- `StartWithClassicDesktopLifetime()` runs Avalonia event loop (blocking)
- `using var` ensures ServiceProvider is disposed after Avalonia exits (disposes singleton services like TelegramBotService)

## Entry Point

App.axaml.cs receives IServiceProvider via constructor injection. The Startup project builds the ServiceProvider and passes it to App.

Constructor injection is used throughout. Avoid using the service provider directly outside of this composition root.
