using System.ComponentModel.DataAnnotations;

namespace Application.Models.Dtos.RestDtos;

public class PlantEditDto
{
    [MaxLength(100)] public string? PlantName { get; set; }

    [MaxLength(100)] public string? PlantType { get; set; }

    [MaxLength(1_000)] public string? PlantNotes { get; set; }

    public DateTime? LastWatered { get; set; }

    [Range(1, 365)] public int? WaterEvery { get; set; }

}