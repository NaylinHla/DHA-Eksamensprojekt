namespace Application.Models.Dtos.BroadcastModels
{
    public class ServerBroadcastsLiveAlertToAlertView : ApplicationBaseDto
    {
        public List<AlertDto> Alerts { get; set; } = [];
        public override string eventType { get; set; } = nameof(ServerBroadcastsLiveAlertToAlertView);
    }

    public class AlertDto
    {
        public string AlertId { get; set; } = "";
        public string AlertName { get; set; } = "";
        public string AlertDesc { get; set; } = "";
        public DateTime AlertTime { get; set; }
        public string? AlertPlantConditionId { get; set; }
        public string? AlertDeviceConditionId { get; set; }
    }
}