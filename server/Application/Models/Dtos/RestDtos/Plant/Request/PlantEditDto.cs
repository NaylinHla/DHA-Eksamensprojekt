using System.ComponentModel.DataAnnotations;

namespace Application.Models.Dtos.RestDtos;

public class PlantEditDto
{
    [MaxLength(100)] public string? PlantName { get; init; }

    [MaxLength(100)] public string? PlantType { get; init; }

    [MaxLength(1_000)] public string? PlantNotes { get; init; }

    public DateTime? LastWatered { get; init; }

    [Range(1, 365)] public int? WaterEvery { get; set; }

}