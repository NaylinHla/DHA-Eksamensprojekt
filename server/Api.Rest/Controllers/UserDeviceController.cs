using Application.Interfaces;
using Application.Models.Dtos.RestDtos;
using Application.Models.Dtos.RestDtos.UserDevice.Request;
using Application.Models.Dtos.RestDtos.UserDevice.Response;
using Microsoft.AspNetCore.Mvc;
using UserDevice = Core.Domain.Entities.UserDevice;

namespace Api.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserDeviceController(IUserDeviceService userDeviceService, ISecurityService securityService) : ControllerBase
{
    
    public const string GetUserDeviceRoute = nameof(GetUserDevice);
    public const string GetAllUserDevicesRoute = nameof(GetAllUserDevices);
    public const string CreateUserDeviceRoute = nameof(CreateUserDevice);
    public const string EditUserDeviceRoute = nameof(EditUserDevice);
    public const string DeleteUserDeviceRoute = nameof(DeleteUserDevice);
    public const string AdminChangesPreferencesRoute = nameof(AdminChangesPreferences);
    
    [HttpGet]
    [Route(GetUserDeviceRoute)]
    public async Task<ActionResult<UserDeviceResponseDto>> GetUserDevice(
        Guid userDeviceId,
        [FromHeader] string authorization)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);
        var userDevice = await userDeviceService.GetUserDeviceAsync(userDeviceId, claims);
        return userDevice is null ? NotFound() : Ok(ToDto(userDevice));
    }
    
    [HttpGet]
    [Route(GetAllUserDevicesRoute)]
    public async Task<ActionResult<UserDeviceResponseDto>> GetAllUserDevices(
        [FromHeader] string authorization)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);
        var userDevice = await userDeviceService.GetAllUserDeviceAsync(claims);
        return Ok(userDevice.Select(ToDto));
    }

    [HttpPost]
    [Route(CreateUserDeviceRoute)]
    public async Task<ActionResult<UserDeviceCreateDto>> CreateUserDevice(
        [FromBody] UserDeviceCreateDto dto,
        [FromHeader] string authorization)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);
        var userDevice = await userDeviceService.CreateUserDeviceAsync(dto, claims);
        return Ok(ToDto(userDevice));
    }

    [HttpPatch]
    [Route(EditUserDeviceRoute)]
    public async Task<ActionResult<UserDeviceEditDto>> EditUserDevice(
        Guid userDeviceId,
        [FromBody] UserDeviceEditDto dto,
        [FromHeader] string authorization)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);
        var updated = await userDeviceService.UpdateUserDeviceAsync(userDeviceId, dto, claims);
        return Ok(ToDto(updated));
    }
    
    [HttpDelete]
    [Route(DeleteUserDeviceRoute)]
    public async Task<ActionResult<UserDeviceEditDto>> DeleteUserDevice(
        Guid userDeviceId,
        [FromHeader] string authorization)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);
        await userDeviceService.DeleteUserDeviceAsync(userDeviceId, claims);
        return Ok();
    }
    
    [HttpPost]
    [Route(AdminChangesPreferencesRoute)]
    public async Task<ActionResult> AdminChangesPreferences([FromBody] AdminChangesPreferencesDto dto,
        [FromHeader] string authorization)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);
        await userDeviceService.UpdateDeviceFeed(dto, claims);
        return Ok();
    }
    
    private static UserDeviceResponseDto ToDto(UserDevice device) => new()
    {
        DeviceId          = device.DeviceId,
        UserId            = device.UserId,
        DeviceName        = device.DeviceName,
        DeviceDescription = device.DeviceDescription,
        CreatedAt         = device.CreatedAt,
        WaitTime          = device.WaitTime
    };
    
}