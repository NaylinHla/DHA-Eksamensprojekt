using System.ComponentModel.DataAnnotations;

namespace Application.Models.Dtos.RestDtos;

public class PlantEditDto
{
    [Required] [MaxLength(100)] public string? PlantName { get; init; } = null!;

    [Required] [MaxLength(100)] public string? PlantType { get; init; } = null!;

    [MaxLength(1_000)] public string? PlantNotes { get; init; } = null!;

    public DateTime? Planted { get; init; }
    public DateTime? LastWatered { get; init; }

    [Range(1, 365)] public int? WaterEvery { get; set; }

    public bool? IsDead { get; set; }
}