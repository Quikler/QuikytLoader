using System;

namespace QuikytLoader.AvaloniaUI.Services;

public interface IUiNotificationService
{
    public event Action<string>? MessageInfoChanged;

    public void SetMessageInfo(string messageInfo);
}
