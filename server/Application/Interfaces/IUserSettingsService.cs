using Application.Models;
using Core.Domain.Entities;

public interface IUserSettingsService
{
    void UpdateSetting(string settingName, bool value, JwtClaims claims);
    bool GetSetting(string settingName, JwtClaims claims);
    UserSettings GetSettings(JwtClaims claims);
}