namespace Application.Models.Dtos.BroadcastModels
{
    public class ServerBroadcastsLiveAlertToAlertView : ApplicationBaseDto
    {
        public List<AlertDto> Alerts { get; set; } = [];
        public override string eventType { get; set; } = nameof(ServerBroadcastsLiveAlertToAlertView);
    }

    public class AlertDto
    {
        public string AlertId { get; set; } = null!;
        public string AlertName { get; set; } = null!;
        public string AlertDesc { get; set; } = null!;
        public DateTime AlertTime { get; set; }
        public string? AlertPlantConditionId { get; set; }
        public string? AlertDeviceConditionId { get; set; }
    }
}