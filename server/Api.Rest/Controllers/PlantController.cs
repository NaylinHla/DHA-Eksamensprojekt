using Application.Interfaces;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;
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
    
    [HttpGet]
    [Route(GetPlantRoute)]
    public async Task<ActionResult<PlantResponseDto>> GetPlant(
        Guid plantId,
        [FromHeader] string authorization)
    {
        securityService.VerifyJwtOrThrow(authorization);
        
        var plant = await plantService.GetPlantByIdAsync(plantId);
        return Ok(ToResponseDto(plant));
    }
    
    [HttpGet]
    [Route(GetPlantsRoute)]
    public async Task<ActionResult<Plant>> GetAllPlants(
        [FromHeader] string authorization)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);

        var plants = await plantService.GetAllPlantsAsync(Guid.Parse(claims.Id));
        return Ok(plants.Select(ToResponseDto).ToList());
    }

    [HttpPost]
    [Route(CreatePlantRoute)]
    public async Task<ActionResult<PlantResponseDto>> CreatePlant(
        [FromBody] PlantCreateDto dto,
        [FromHeader] string authorization)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);
        
        var userId  = Guid.Parse(claims.Id);
        
        var plant = new Plant
        {
            PlantId     = Guid.NewGuid(),
            PlantName   = dto.PlantName,
            PlantType   = dto.PlantType,
            PlantNotes  = dto.PlantNotes,
            Planted     = dto.Planted,
            LastWatered = null,
            WaterEvery  = dto.WaterEvery,
            IsDead      = dto.IsDead
        };
        
        plant = await plantService.CreatePlantAsync(userId, plant);
        
        return CreatedAtAction(nameof(GetPlant), new { plantId = plant.PlantId }, ToResponseDto(plant));
    }

    [HttpPatch]
    [Route(EditPlantRoute)]
    public async Task<ActionResult<PlantEditDto>> EditPlant(
        Guid plantId,
        [FromBody] PlantEditDto dto,
        [FromHeader] string authorization)
    {
        securityService.VerifyJwtOrThrow(authorization);
        
        var plant = await plantService.EditPlantAsync(new Plant
        {
            PlantId      = plantId,
            PlantName    = dto.PlantName,
            PlantType    = dto.PlantType,
            PlantNotes   = dto.PlantNotes,
            WaterEvery   = dto.WaterEvery
        });
        
        return Ok(ToResponseDto(plant));
    }

    [HttpPatch]
    [Route(PlantIsDeadRoute)]
    public async Task<IActionResult> MarkPlantAsDead(
        Guid plantId,
        [FromHeader] string authorization)
    {
        securityService.VerifyJwtOrThrow(authorization);
        await plantService.MarkPlantAsDeadAsync(plantId);
        return Ok();
    }
    
    [HttpPatch]
    [Route(WaterPlantRoute)]
    public async Task<IActionResult> WaterPlant(
        Guid   plantId,
        [FromHeader] string authorization)
    {
        securityService.VerifyJwtOrThrow(authorization);

        await plantService.WaterPlantAsync(plantId);
        return Ok();
    }

    [HttpPatch]
    [Route(WaterAllPlantsRoute)]
    public async Task<IActionResult> WaterAllPlants(
        [FromHeader] string authorization)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);

        await plantService.WaterAllPlantsAsync(Guid.Parse(claims.Id));
        return Ok();
    }
    
    
    private static PlantResponseDto ToResponseDto(Plant p) => new()
    {
        PlantId      = p.PlantId,
        PlantName    = p.PlantName,
        PlantType    = p.PlantType,
        PlantNotes   = p.PlantNotes,
        Planted      = p.Planted,
        LastWatered  = p.LastWatered,
        WaterEvery   = p.WaterEvery,
        IsDead       = p.IsDead
    };
}