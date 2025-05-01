using Application.Interfaces;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models.Dtos.MqttDtos.Response;
using Application.Models.Dtos.RestDtos;
using Application.Models.Dtos.RestDtos.SensorHistory;
using Application.Models.Dtos.RestDtos.UserDevice;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GreenhouseDeviceController(
    IGreenhouseDeviceService greenhouseDeviceService,
    IConnectionManager connectionManager,
    ISecurityService securityService) : ControllerBase
{
    public const string GetSensorDataRoute = nameof(GetSensorDataByDeviceId);
    
    public const string GetAllUserDevicesRoute = nameof(GetAllUserDevices);
    
    public const string GetRecentSensorDataForAllUserDeviceRoute = nameof(GetRecentSensorDataForAllUserDevice);
    
    public const string AdminChangesPreferencesRoute = nameof(AdminChangesPreferences);

    [HttpGet]
    [Route(GetSensorDataRoute)]
    public async Task<ActionResult<IEnumerable<GetAllSensorHistoryByDeviceIdDto>>> GetSensorDataByDeviceId(
        [FromQuery] Guid deviceId,
        [FromHeader] string authorization)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);
        var data = await greenhouseDeviceService.GetSensorHistoryByDeviceIdAndBroadcast(deviceId, claims);
        return Ok(data);
    }
    
    [HttpGet]
    [Route(GetRecentSensorDataForAllUserDeviceRoute)]
    public async Task<ActionResult<GetRecentSensorDataForAllUserDeviceDto>> GetRecentSensorDataForAllUserDevice(
        [FromHeader] string authorization)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);
        var data = await greenhouseDeviceService.GetRecentSensorDataForAllUserDevicesAsync(claims);
        return Ok(data);
    }

    [HttpGet]
    [Route(GetAllUserDevicesRoute)]
    public async Task<ActionResult<IEnumerable<GetAllUserDeviceDto>>> GetAllUserDevices(
        [FromHeader] string authorization)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);
        var data = await greenhouseDeviceService.GetAllUserDevice(claims);
        return Ok(data);
    }

    [HttpPost]
    [Route(AdminChangesPreferencesRoute)]
    public async Task<ActionResult> AdminChangesPreferences([FromBody] AdminChangesPreferencesDto dto,
        [FromHeader] string authorization)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);
        await greenhouseDeviceService.UpdateDeviceFeed(dto, claims);
        return Ok();
    }

    public const string DeleteDataRoute = nameof(DeleteData);

    [HttpDelete]
    [Route(DeleteDataRoute)]
    public async Task<ActionResult> DeleteData([FromHeader] string authorization)
    {
        var jwt = securityService.VerifyJwtOrThrow(authorization);

        await greenhouseDeviceService.DeleteDataAndBroadcast(jwt);

        return Ok();
    }
}