using Application.Interfaces;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertController(IAlertService alertService, ISecurityService securityService) : ControllerBase
{
    public const string CreateAlertRoute = nameof(CreateAlert);
    public const string GetAlertsRoute = nameof(GetAlerts);

    [HttpGet]
    [Route(GetAlertsRoute)]
    public async Task<ActionResult<List<Alert>>> GetAlerts(
        [FromHeader] string authorization,
        [FromQuery] int? year = null)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);
        var alerts = await alertService.GetAlertsAsync(Guid.Parse(claims.Id), year);
        return Ok(alerts);
    }

    [HttpPost]
    [Route(CreateAlertRoute)]
    public async Task<ActionResult<AlertResponseDto>> CreateAlert(
        [FromBody] AlertCreate dto,
        [FromHeader] string authorization)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);
        var alert = await alertService.CreateAlertAsync(
            Guid.Parse(claims.Id),
            dto.AlertName,
            dto.AlertDesc,
            dto.AlertPlant
        );

        var response = new AlertResponseDto
        {
            AlertId = alert.AlertId,
            AlertName = alert.AlertName,
            AlertDesc = alert.AlertDesc,
            AlertTime = alert.AlertTime,
            AlertPlant = alert.AlertPlant
        };
        return Ok(response);
    }
}