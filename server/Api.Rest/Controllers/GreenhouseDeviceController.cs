using Application.Interfaces;
using Application.Models.Dtos.MqttDtos.Response;
using Application.Models.Dtos.RestDtos;
using Application.Models.Dtos.RestDtos.SensorHistory;
using Application.Models.Dtos.RestDtos.UserDevice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GreenhouseDeviceController(
    IGreenhouseDeviceService greenhouseDeviceService,
    ISecurityService securityService) : ControllerBase
{
    public const string GetSensorDataRoute = nameof(GetAllSensorHistoryByDeviceAndTimePeriodIdDto);
    
    public const string GetAllUserDevicesRoute = nameof(GetAllUserDevices);
    
    public const string GetRecentSensorDataForAllUserDeviceRoute = nameof(GetRecentSensorDataForAllUserDevice);
    
    public const string AdminChangesPreferencesRoute = nameof(AdminChangesPreferences);
    
    public const string DeleteDataRoute = nameof(DeleteDataFromSpecificDevice);

    [HttpGet]
    [Route(GetSensorDataRoute)]
    public async Task<ActionResult<List<GetAllSensorHistoryByDeviceIdDto>>> GetAllSensorHistoryByDeviceAndTimePeriodIdDto(
        Guid deviceId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromHeader] string authorization)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);
        var result = await greenhouseDeviceService.GetSensorHistoryByDeviceId(deviceId, from, to, claims);
        return Ok(result);
    }
    
    [HttpGet]
    [Route(GetRecentSensorDataForAllUserDeviceRoute)]
    public async Task<ActionResult<GetRecentSensorDataForAllUserDeviceDto>> GetRecentSensorDataForAllUserDevice(
        [FromHeader] string authorization)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);
        var data = await greenhouseDeviceService.GetRecentSensorDataForAllUserDevicesAsync(claims);
        if (data.SensorHistoryWithDeviceRecords.Count == 0)
        {
            return NoContent();
        }
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
    
    [HttpDelete]
    [Route(DeleteDataRoute)]
    public async Task<ActionResult> DeleteDataFromSpecificDevice([FromQuery] Guid deviceId, [FromHeader] string authorization)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);

        await greenhouseDeviceService.DeleteDataFromSpecificDeviceAndBroadcast(deviceId, claims);

        return Ok();
    }
}