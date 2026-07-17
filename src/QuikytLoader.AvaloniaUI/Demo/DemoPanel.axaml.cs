using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace QuikytLoader.AvaloniaUI.Demo;

public partial class DemoPanel : UserControl
{
    public DemoPanel()
    {
        InitializeComponent();

        if (!Design.IsDesignMode && Avalonia.Application.Current is App app)
            DataContext = app.Services.GetRequiredService<DemoPanelViewModel>();
    }
}
