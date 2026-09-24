using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using QuikytLoader.Application.Interfaces.Temp;
using QuikytLoader.AvaloniaUI.ViewModels;
using QuikytLoader.AvaloniaUI.Views;
using System;

namespace QuikytLoader.AvaloniaUI;

public partial class App(IServiceProvider serviceProvider) : Avalonia.Application
{
    public IServiceProvider Services => serviceProvider;

    // Not null after registration
    public MainWindow MainWindow { get; private set; } = null!;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Handle SIGTERM
            desktop.Exit += (_, _) =>
                serviceProvider.GetRequiredService<ITempDirectoryService>().Delete();

            desktop.MainWindow = MainWindow = new MainWindow
            {
                DataContext = serviceProvider.GetRequiredService<AppViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
