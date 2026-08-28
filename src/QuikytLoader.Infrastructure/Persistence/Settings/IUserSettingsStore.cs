using QuikytLoader.Application.DTOs;

namespace QuikytLoader.Infrastructure.Persistence.Settings;

internal interface IUserSettingsStore
{
    UserSettingsDto Load();

    void Save(UserSettingsDto settings);
}
