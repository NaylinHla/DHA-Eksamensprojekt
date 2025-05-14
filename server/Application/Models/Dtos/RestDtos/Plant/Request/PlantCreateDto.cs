using System.ComponentModel.DataAnnotations;

namespace Application.Models.Dtos.RestDtos;

public sealed class PlantCreateDto
{
    [Required] [MaxLength(100)] public string PlantName { get; init; } = null!;

    [Required] [MaxLength(100)] public string PlantType { get; init; } = null!;

    [MaxLength(1_000)] public string? PlantNotes { get; init; }

    public required DateTime? Planted { get; init; } = DateTime.UtcNow.Date;

    [Range(1, 365)] public int? WaterEvery { get; set; }

    public bool IsDead { get; set; }
}