using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models;
using Core.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class AlertService : IAlertService
{
    private readonly IAlertRepository _alertRepo;
    private readonly IConnectionManager _ws;

    public AlertService(IAlertRepository alertRepo, IConnectionManager ws)
    {
        _alertRepo = alertRepo;
        _ws = ws;
    }

    public async Task<Alert> CreateAlertAsync(Guid userId, string title, string description, Guid? plantId = null)
    {
        var alert = new Alert
        {
            AlertID = Guid.NewGuid(),
            AlertUserId = userId,
            AlertName = title,
            AlertDesc = description,
            AlertTime = DateTime.UtcNow,
            AlertPlant = plantId
        };

        var savedAlert = await _alertRepo.AddAlertAsync(alert);

        await _ws.BroadcastToTopic($"alerts-{userId}", new
        {
            type = "alert",
            data = new
            {
                alert.AlertID,
                alert.AlertName,
                alert.AlertDesc,
                alert.AlertTime,
                alert.AlertPlant
            }
        });

        return savedAlert;
    }

    public Task<List<Alert>> GetAlertsAsync(Guid userId, int? year = null)
        => _alertRepo.GetAlertsAsync(userId, year);
}


