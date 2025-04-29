namespace Application.Models.Dtos.RestDtos;

public class AlertResponseDto
{
    public Guid AlertID { get; set; }
    public string AlertName { get; set; }
    public string AlertDesc { get; set; }
    public DateTime AlertTime { get; set; }
    public Guid? AlertPlant { get; set; }
}
