using QuikytLoader.Application.DTOs;
using QuikytLoader.Application.Interfaces.Settings;

namespace QuikytLoader.Infrastructure.Persistence.Settings;

internal class UserSettings(IUserSettingsStore userSettingsStore) : IUserSettings
{
    private UserSettingsDto _current = userSettingsStore.Load();

    public UserSettingsDto Current
    {
        get => _current;
        set
        {
            var oldSettings = _current;

            if (Equals(oldSettings, value))
                return;

            _current = value;

            userSettingsStore.Save(_current);

            Changed?.Invoke(
                new UserSettingsChangedEventArgs(oldSettings, _current));
        }
    }

    public event Action<UserSettingsChangedEventArgs>? Changed;
}
