using System.ComponentModel.DataAnnotations;

namespace Application.Models.Dtos.RestDtos;

public sealed class PlantCreateDto
{
    [Required] [MaxLength(100)] public string PlantName { get; set; } = null!;

    [Required] [MaxLength(50)] public string PlantType { get; set; } = null!;

    [MaxLength(1_000)] public string? PlantNotes { get; set; }

    public required DateTime? Planted { get; set; } = DateTime.UtcNow.Date;

    [Range(1, 365)] public int? WaterEvery { get; set; }

    public bool IsDead { get; set; }
}