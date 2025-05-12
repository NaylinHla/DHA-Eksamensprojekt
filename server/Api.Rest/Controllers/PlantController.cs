using Application.Interfaces;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;
using Infrastructure.Logging;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlantController(IPlantService plantService, ISecurityService securityService) : ControllerBase
{
    public const string GetPlantRoute = nameof(GetPlant);
    public const string GetPlantsRoute = nameof(GetAllPlants);
    public const string CreatePlantRoute = nameof(CreatePlant);
    public const string EditPlantRoute = nameof(EditPlant);
    public const string PlantIsDeadRoute = nameof(MarkPlantAsDead);
    public const string WaterPlantRoute = nameof(WaterPlant);
    public const string WaterAllPlantsRoute = nameof(WaterAllPlants);
    public const string DeletePlantRoute = nameof(DeletePlant);

    [HttpGet]
    [Route(GetPlantRoute)]
    public async Task<ActionResult<PlantResponseDto>> GetPlant(
        Guid plantId,
        [FromHeader] string authorization)
    {
        MonitorService.Log.Debug("Entered GetPlant method in PlantController");
        var claims = securityService.VerifyJwtOrThrow(authorization);
        var plant = await plantService.GetPlantByIdAsync(plantId, claims);
        return plant is null ? NotFound() : Ok(ToDto(plant));
    }

    [HttpGet]
    [Route(GetPlantsRoute)]
    public async Task<ActionResult<IEnumerable<PlantResponseDto>>> GetAllPlants(
        Guid userId,
        [FromHeader] string authorization)
    {
        MonitorService.Log.Debug("Entered GetAllPlants method in PlantController");
        var claims = securityService.VerifyJwtOrThrow(authorization);
        
        var plants = await plantService.GetAllPlantsAsync(userId, claims);
        return Ok(plants.Select(ToDto));
    }

    [HttpPost]
    [Route(CreatePlantRoute)]
    public async Task<ActionResult<PlantResponseDto>> CreatePlant(
        [FromBody] PlantCreateDto dto,
        [FromHeader] string authorization)
    {
        MonitorService.Log.Debug("Entered Create Plant method in PlantController");
        var claims = securityService.VerifyJwtOrThrow(authorization);
        var plant = await plantService.CreatePlantAsync(Guid.Parse(claims.Id), dto);
        return Ok(ToDto(plant));
    }

    [HttpDelete]
    [Route(DeletePlantRoute)]
    public async Task<ActionResult<PlantResponseDto>> DeletePlant(
        Guid plantId,
        [FromHeader] string authorization)
    {
        MonitorService.Log.Debug("Entered Delete Plant method in PlantController");
        var claims = securityService.VerifyJwtOrThrow(authorization);
        await plantService.DeletePlantAsync(plantId, claims);
        return Ok();
    }

    [HttpPatch]
    [Route(EditPlantRoute)]
    public async Task<ActionResult<PlantEditDto>> EditPlant(
        Guid userId,
        Guid plantId,
        [FromBody] PlantEditDto dto,
        [FromHeader] string authorization)
    {
        MonitorService.Log.Debug("Entered Edit Plant method in PlantController");
        var claims = securityService.VerifyJwtOrThrow(authorization);
        var updated = await plantService.EditPlantAsync(plantId, dto, claims);
        return Ok(ToDto(updated));
    }

    [HttpPatch]
    [Route(PlantIsDeadRoute)]
    public async Task<IActionResult> MarkPlantAsDead(
        Guid plantId,
        [FromHeader] string authorization)
    {
        MonitorService.Log.Debug("Entered Mark Plant As Dead method in PlantController");
        var claims = securityService.VerifyJwtOrThrow(authorization);
        await plantService.MarkPlantAsDeadAsync(plantId, claims);
        return Ok();
    }

    [HttpPatch]
    [Route(WaterPlantRoute)]
    public async Task<IActionResult> WaterPlant(
        Guid plantId,
        [FromHeader] string authorization)
    {
        MonitorService.Log.Debug("Entered Water Plant method in PlantController");
        var claims = securityService.VerifyJwtOrThrow(authorization);
        await plantService.WaterPlantAsync(plantId, claims);
        return Ok();
    }

    [HttpPatch]
    [Route(WaterAllPlantsRoute)]
    public async Task<IActionResult> WaterAllPlants(
        Guid userId,
        [FromHeader] string authorization)
    {
        MonitorService.Log.Debug("Entered Water All Plants method in PlantController");
        var claims = securityService.VerifyJwtOrThrow(authorization);
        await plantService.WaterAllPlantsAsync(userId, claims);
        return Ok();
    }


    private static PlantResponseDto ToDto(Plant p)
    {
        return new PlantResponseDto
        {
            PlantId = p.PlantId,
            PlantName = p.PlantName,
            PlantType = p.PlantType,
            PlantNotes = p.PlantNotes,
            Planted = p.Planted,
            LastWatered = p.LastWatered,
            WaterEvery = p.WaterEvery,
            IsDead = p.IsDead
        };
    }
}