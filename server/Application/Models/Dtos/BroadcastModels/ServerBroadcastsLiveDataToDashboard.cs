using Core.Domain.Entities;

namespace Application.Models.Dtos.BroadcastModels;

public class ServerBroadcastsLiveDataToDashboard : ApplicationBaseDto
{
    public List<Devicelog> Logs { get; set; }
    public override string eventType { get; set; } = nameof(ServerBroadcastsLiveDataToDashboard);
}