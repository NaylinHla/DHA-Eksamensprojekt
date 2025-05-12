namespace Application.Models.Dtos.RestDtos;

public class PlantResponseDto
{
    public required Guid PlantId { get; set; }
    public required string PlantName { get; set; }
    public required string PlantType { get; set; }
    public string? PlantNotes { get; set; } = null!;
    public DateTime? Planted { get; set; }
    public DateTime? LastWatered { get; set; }
    public int? WaterEvery { get; set; }
    public bool IsDead { get; set; }
}