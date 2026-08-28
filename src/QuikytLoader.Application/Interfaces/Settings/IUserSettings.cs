using QuikytLoader.Application.DTOs;

namespace QuikytLoader.Application.Interfaces.Settings;

public interface IUserSettings
{
    UserSettingsDto Current { get; set; }

    event Action<UserSettingsChangedEventArgs>? Changed;
}

public sealed record UserSettingsChangedEventArgs(
    UserSettingsDto OldSettings,
    UserSettingsDto NewSettings);
