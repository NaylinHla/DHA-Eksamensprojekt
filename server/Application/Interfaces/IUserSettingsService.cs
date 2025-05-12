using Application.Models;
using Core.Domain.Entities;

namespace Application.Interfaces;

public interface IUserSettingsService
{
    void UpdateSetting(string settingName, bool value, JwtClaims claims);
    UserSettings GetSettings(JwtClaims claims);
}