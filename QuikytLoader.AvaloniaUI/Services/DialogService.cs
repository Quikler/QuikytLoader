using System.Threading.Tasks;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace QuikytLoader.AvaloniaUI.Services;

public class DialogService : IDialogService
{
    public async Task<bool> ShowConfirmationAsync(string title, string message)
    {
        var mainWindow = (Avalonia.Application.Current as App)!.MainWindow;
        var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.YesNo);
        return await box.ShowWindowDialogAsync(mainWindow) == ButtonResult.Yes;
    }

    public async Task ShowWarningAsync(string title, string message)
    {
        var mainWindow = (Avalonia.Application.Current as App)!.MainWindow;
        var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, Icon.Warning);
        await box.ShowWindowDialogAsync(mainWindow);
    }
}
