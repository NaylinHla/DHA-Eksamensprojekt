using System.ComponentModel.DataAnnotations;

namespace Application.Models.Dtos.RestDtos;

public class ConditionAlertPlantCreateDto
{
    [Required]
    public Guid PlantId { get; set; }
}
