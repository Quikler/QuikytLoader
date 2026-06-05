using System;

namespace QuikytLoader.AvaloniaUI.Services;

public class UiNotificationService : IUiNotificationService
{
    public event Action<string>? MessageInfoChanged;

    public void SetMessageInfo(string messageInfo) => MessageInfoChanged?.Invoke(messageInfo);
}
