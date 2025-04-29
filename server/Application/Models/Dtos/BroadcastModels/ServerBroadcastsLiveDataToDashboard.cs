using Application.Models.Dtos.MqttDtos.Response;

namespace Application.Models.Dtos.BroadcastModels;

public class ServerBroadcastsLiveDataToDashboard : ApplicationBaseDto
{
    public List<GetAllSensorHistoryByDeviceIdDto> Logs { get; set; }
    public override string eventType { get; set; } = nameof(ServerBroadcastsLiveDataToDashboard);
}