using System.Threading.Tasks;

namespace QuikytLoader.AvaloniaUI.Services;

public interface IDialogService
{
    Task<bool> ShowConfirmationAsync(string title, string message);
}
