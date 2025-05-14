using Application.Interfaces;
using Application.Models.Dtos.RestDtos;
using Infrastructure.Logging;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertConditionController(IAlertConditionService alertConditionService, ISecurityService securityService)
    : ControllerBase
{
    public const string GetConditionAlertPlantRoute = nameof(GetConditionAlertPlant);
    public const string GetConditionAlertPlantsRoute = nameof(GetConditionAlertPlants);
    public const string CreateConditionAlertPlantRoute = nameof(CreateConditionAlertPlant);
    public const string DeleteConditionAlertPlantRoute = nameof(DeleteConditionAlertPlant);

    public const string GetConditionAlertUserDeviceRoute = nameof(GetConditionAlertUserDevice);
    public const string GetConditionAlertUserDevicesRoute = nameof(GetConditionAlertUserDevices);
    public const string CreateConditionAlertUserDeviceRoute = nameof(CreateConditionAlertUserDevice);
    public const string EditConditionAlertUserDeviceRoute = nameof(EditConditionAlertUserDevice);
    public const string DeleteConditionAlertUserDeviceRoute = nameof(DeleteConditionAlertUserDevice);

    // PLANT ALERT CONDITION ROUTES

    [HttpGet]
    [Route(GetConditionAlertPlantRoute)]
    public async Task<ActionResult<ConditionAlertPlantResponseDto>> GetConditionAlertPlant(Guid plantId,
        [FromHeader] string authorization)
    {
        MonitorService.Log.Debug("Entered GetConditionAlertPlant in AlertConditionController");
        var claims = securityService.VerifyJwtOrThrow(authorization);
        var result = await alertConditionService.GetConditionAlertPlantByIdAsync(plantId, claims);
        return Ok(result);
    }

    [HttpGet]
    [Route(GetConditionAlertPlantsRoute)]
    public async Task<ActionResult<IEnumerable<ConditionAlertPlantResponseDto>>> GetConditionAlertPlants(Guid userId,
        [FromHeader] string authorization)
    {
        MonitorService.Log.Debug("Entered GetConditionAlertPlants in AlertConditionController");
        var claims = securityService.VerifyJwtOrThrow(authorization);
        var result = await alertConditionService.GetAllConditionAlertPlantsAsync(userId, claims);
        return Ok(result);
    }

    [HttpPost]
    [Route(CreateConditionAlertPlantRoute)]
    public async Task<ActionResult<ConditionAlertPlantResponseDto>> CreateConditionAlertPlant([FromBody] ConditionAlertPlantCreateDto dto,
        [FromHeader] string authorization)
    {
        MonitorService.Log.Debug("Entered CreateConditionAlertPlant in AlertConditionController");
        var claims = securityService.VerifyJwtOrThrow(authorization);
        var created = await alertConditionService.CreateConditionAlertPlantAsync(dto, claims);
        return Ok(created);
    }

    [HttpDelete]
    [Route(DeleteConditionAlertPlantRoute)]
    public async Task<IActionResult> DeleteConditionAlertPlant(Guid conditionId, [FromHeader] string authorization)
    {
        MonitorService.Log.Debug("Entered DeleteConditionAlertPlant in AlertConditionController");
        var claims = securityService.VerifyJwtOrThrow(authorization);
        await alertConditionService.DeleteConditionAlertPlantAsync(conditionId, claims);
        return Ok();
    }

    // USER DEVICE ALERT CONDITION ROUTES

    [HttpGet]
    [Route(GetConditionAlertUserDeviceRoute)]
    public async Task<ActionResult<ConditionAlertUserDeviceResponseDto>> GetConditionAlertUserDevice(Guid userDeviceId,
        [FromHeader] string authorization)
    {
        MonitorService.Log.Debug("Entered GetConditionAlertUserDevice in AlertConditionController");
        var claims = securityService.VerifyJwtOrThrow(authorization);
        var result = await alertConditionService.GetConditionAlertUserDeviceByIdAsync(userDeviceId, claims);
        return Ok(result);
    }

    [HttpGet]
    [Route(GetConditionAlertUserDevicesRoute)]
    public async Task<ActionResult<IEnumerable<ConditionAlertUserDeviceResponseDto>>> GetConditionAlertUserDevices(
        Guid userId, [FromHeader] string authorization)
    {
        MonitorService.Log.Debug("Entered GetConditionAlertUserDevices in AlertConditionController");
        var claims = securityService.VerifyJwtOrThrow(authorization);
        var result = await alertConditionService.GetAllConditionAlertUserDevicesAsync(userId, claims);
        return Ok(result);
    }

    [HttpPost]
    [Route(CreateConditionAlertUserDeviceRoute)]
    public async Task<ActionResult<ConditionAlertUserDeviceResponseDto>> CreateConditionAlertUserDevice(
        [FromBody] ConditionAlertUserDeviceCreateDto dto, [FromHeader] string authorization)
    {
        MonitorService.Log.Debug("Entered CreateConditionAlertUserDevice in AlertConditionController");
        var claims = securityService.VerifyJwtOrThrow(authorization);
        var created = await alertConditionService.CreateConditionAlertUserDeviceAsync(dto, claims);
        return Ok(created);
    }

    [HttpPatch]
    [Route(EditConditionAlertUserDeviceRoute)]
    public async Task<ActionResult<ConditionAlertUserDeviceResponseDto>> EditConditionAlertUserDevice(
        [FromBody] ConditionAlertUserDeviceEditDto dto, [FromHeader] string authorization)
    {
        MonitorService.Log.Debug("Entered EditConditionAlertUserDevice in AlertConditionController");
        var claims = securityService.VerifyJwtOrThrow(authorization);
        var updated = await alertConditionService.EditConditionAlertUserDeviceAsync(dto, claims);
        return Ok(updated);
    }

    [HttpDelete]
    [Route(DeleteConditionAlertUserDeviceRoute)]
    public async Task<IActionResult> DeleteConditionAlertUserDevice(Guid conditionId, [FromHeader] string authorization)
    {
        MonitorService.Log.Debug("Entered DeleteConditionAlertUserDevice in AlertConditionController");
        var claims = securityService.VerifyJwtOrThrow(authorization);
        await alertConditionService.DeleteConditionAlertUserDeviceAsync(conditionId, claims);
        return Ok();
    }
}