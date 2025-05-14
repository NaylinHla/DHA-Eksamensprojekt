using System.ComponentModel.DataAnnotations;

namespace Application.Models.Dtos.RestDtos;

public class AlertCreateDto
{
    [MaxLength(100)] 
    public required string AlertName { get; set; }

    public required string AlertDesc { get; set; }

    public Guid? AlertConditionId { get; set; }
    
    public bool IsPlantCondition { get; set; }

    public Guid? AlertUser { get; set; }
}