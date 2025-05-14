namespace Application.Models.Dtos.RestDtos
{
    public class AlertResponseDto
    {
        public Guid AlertId { get; set; }
        public required string AlertName { get; set; }
        public required string AlertDesc { get; set; }
        public DateTime AlertTime { get; set; }
        public Guid? AlertPlantConditionId { get; set; }
        public Guid? AlertDeviceConditionId { get; set; }
    }
}