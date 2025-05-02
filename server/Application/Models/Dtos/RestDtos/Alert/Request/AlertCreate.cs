namespace Application.Models.Dtos.RestDtos;

public class AlertCreate
{
    public required string AlertName { get; set; }

    public required string AlertDesc { get; set; }

    public Guid? AlertPlant { get; set; }
    
    public Guid? AlertUser { get; set; }
}