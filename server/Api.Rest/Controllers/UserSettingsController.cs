using Application.Interfaces;
using Application.Models;
using Application.Models.Dtos.RestDtos;
using Application.Models.Dtos.RestDtos.UserSettings.Response;
using Application.Models.Dtos.UserSettings;
using Infrastructure.Logging;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserSettingsController(IUserSettingsService service, ISecurityService securityService) : ControllerBase
{
    [HttpPatch("{settingName}")]
    public IActionResult PatchSetting(
        string settingName,
        [FromBody] UpdateUserSettingDto dto,
        [FromHeader] string authorization)
    {
        MonitorService.Log.Debug("Entered PatchSetting in UserSettingsController");

        var token = authorization.StartsWith("Bearer ") ? authorization.Substring(7) : authorization;
        var claims = securityService.VerifyJwtOrThrow(token);

        service.UpdateSetting(settingName, dto.Value, claims);
        return NoContent();
    }

    [HttpGet]
    public ActionResult<UserSettingsResponseDto> GetAllSettings([FromHeader] string authorization)
    {
            MonitorService.Log.Debug("Entered GetAllSettings in UserSettingsController");
            
            var token = authorization.StartsWith("Bearer ") ? authorization.Substring(7) : authorization;
            var claims = securityService.VerifyJwtOrThrow(token);

            var settings = service.GetSettings(claims);

            return Ok(new UserSettingsResponseDto
            {
                Celsius = settings.Celsius,
                DarkTheme = settings.DarkTheme,
                ConfirmDialog = settings.ConfirmDialog,
                SecretMode = settings.SecretMode
            });
    }
}