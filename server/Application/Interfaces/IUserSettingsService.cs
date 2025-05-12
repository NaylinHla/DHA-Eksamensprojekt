using Application.Models;

public interface IUserSettingsService
{
    void UpdateSetting(string settingName, bool value, JwtClaims claims);
    bool GetSetting(string settingName, JwtClaims claims);
}