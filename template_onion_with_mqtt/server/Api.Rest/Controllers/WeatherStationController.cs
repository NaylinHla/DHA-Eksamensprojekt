using Application.Interfaces;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest.Controllers;

[ApiController]
public class WeatherStationController(
    IWeatherStationService weatherStationService,
    IConnectionManager connectionManager,
    ISecurityService securityService) : ControllerBase
{
    public const string GetLogsRoute = nameof(GetLogs);


    public const string AdminChangesPreferencesRoute = nameof(AdminChangesPreferences);

    [HttpGet]
    [Route(GetLogsRoute)]
    public async Task<ActionResult<IEnumerable<Devicelog>>> GetLogs([FromHeader] string authorization)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);
        var feed = weatherStationService.GetDeviceFeed(claims);
        return Ok(feed);
    }

    [HttpPost]
    [Route(AdminChangesPreferencesRoute)]
    public async Task<ActionResult> AdminChangesPreferences([FromBody] AdminChangesPreferencesDto dto,
        [FromHeader] string authorization)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);
        await weatherStationService.UpdateDeviceFeed(dto, claims);
        return Ok();
    }

    public const string DeleteDataRoute = nameof(DeleteData);
    [HttpDelete]
    [Route(DeleteDataRoute)]
    public async Task<ActionResult> DeleteData([FromHeader]string authorization)
    {
        var jwt = securityService.VerifyJwtOrThrow(authorization);

        await weatherStationService.DeleteDataAndBroadcast(jwt);
        
        return Ok();
    }
    
}