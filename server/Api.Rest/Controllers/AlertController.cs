using Application.Interfaces;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertController : ControllerBase
{
    private readonly IAlertService _alertService;
    private readonly ISecurityService _securityService;

    public AlertController(IAlertService alertService, ISecurityService securityService)
    {
        _alertService = alertService;
        _securityService = securityService;
    }

    public const string CreateAlertRoute = nameof(CreateAlert);

    
    
    [HttpGet("GetAlerts")]
    public async Task<ActionResult<List<Alert>>> GetAlerts(
        [FromHeader] string authorization,
        [FromQuery] int? year = null)
    {
        var claims = _securityService.VerifyJwtOrThrow(authorization);

        var alerts = await _alertService.GetAlertsAsync(Guid.Parse(claims.Id), year);
        return Ok(alerts);
    }

    [HttpPost]
    [Route(CreateAlertRoute)]
    public async Task<ActionResult<AlertResponseDto>> CreateAlert(
        [FromBody] AlertCreate dto,
        [FromHeader] string authorization)
    {
        var claims = _securityService.VerifyJwtOrThrow(authorization);
        var alert = await _alertService.CreateAlertAsync(
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
