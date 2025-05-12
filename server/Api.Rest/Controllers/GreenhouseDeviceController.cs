using Application.Interfaces;
using Application.Models.Dtos.MqttDtos.Response;
using Application.Models.Dtos.RestDtos.SensorHistory;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GreenhouseDeviceController(
    IGreenhouseDeviceService greenhouseDeviceService,
    ISecurityService securityService) : ControllerBase
{
    public const string GetSensorDataRoute = nameof(GetAllSensorHistoryByDeviceAndTimePeriodIdDto);

    public const string GetRecentSensorDataForAllUserDeviceRoute = nameof(GetRecentSensorDataForAllUserDevice);

    public const string DeleteDataRoute = nameof(DeleteDataFromSpecificDevice);

    [HttpGet]
    [Route(GetSensorDataRoute)]
    public async Task<ActionResult<List<GetAllSensorHistoryByDeviceIdDto>>>
        GetAllSensorHistoryByDeviceAndTimePeriodIdDto(
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
        if (data.SensorHistoryWithDeviceRecords.Count == 0) return NoContent();

        return Ok(data);
    }

    [HttpDelete]
    [Route(DeleteDataRoute)]
    public async Task<ActionResult> DeleteDataFromSpecificDevice([FromQuery] Guid deviceId,
        [FromHeader] string authorization)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);

        await greenhouseDeviceService.DeleteDataFromSpecificDeviceAndBroadcast(deviceId, claims);

        return Ok();
    }
}