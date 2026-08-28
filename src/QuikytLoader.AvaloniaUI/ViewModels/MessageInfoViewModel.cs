using CommunityToolkit.Mvvm.ComponentModel;
using QuikytLoader.AvaloniaUI.Services;

namespace QuikytLoader.AvaloniaUI.ViewModels;

public partial class MessageInfoViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _messageInfo = "Ready";

    public MessageInfoViewModel(IUiNotificationService uiNotificationService)
    {
        uiNotificationService.MessageInfoChanged += messageInfo => MessageInfo = messageInfo;
    }
}
