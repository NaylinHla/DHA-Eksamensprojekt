using System.ComponentModel.DataAnnotations;
using System.Security.Authentication;
using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Core.Domain.Entities;
using Infrastructure.Logging;
using Application.Models;

namespace Application.Services;

public class UserSettingsService(IUserSettingsRepository repository) : IUserSettingsService
{
    public void UpdateSetting(string settingName, bool value, JwtClaims claims)
    {
        var userId = Guid.Parse(claims.Id);
        var settings = repository.GetByUserId(userId)
                       ?? throw new KeyNotFoundException("User settings not found");

        switch (settingName.ToLowerInvariant())
        {
            case "celsius": settings.Celsius = value; break;
            case "darktheme": settings.DarkTheme = value; break;
            case "confirmdialog": settings.ConfirmDialog = value; break;
            case "secretmode": settings.SecretMode = value; break;
            default:
                MonitorService.Log.Error($"Invalid setting name: {settingName}");
                throw new ValidationException("Invalid setting name");
        }

        repository.Update(settings);
    }
    
    public bool GetSetting(string settingName, JwtClaims claims)
    {
        var userId = Guid.Parse(claims.Id);
        var settings = repository.GetByUserId(userId)
                       ?? throw new KeyNotFoundException("User settings not found");

        return settingName.ToLowerInvariant() switch
        {
            "celsius" => settings.Celsius,
            "darktheme" => settings.DarkTheme,
            "confirmdialog" => settings.ConfirmDialog,
            "secretmode" => settings.SecretMode,
            _ => throw new ValidationException("Invalid setting name")
        };
    }
}