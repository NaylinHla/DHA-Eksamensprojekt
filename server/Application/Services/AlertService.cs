using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;
using Infrastructure.Logging;

namespace Application.Services;

public class AlertService(IAlertRepository alertRepo, IConnectionManager ws) : IAlertService
{
    public async Task<Alert> CreateAlertAsync(Guid userId, AlertCreateDto dto)
    {
        MonitorService.Log.Debug("Entered CreateAlertAsync method in AlertService");

        if (dto.AlertConditionId == null)
        {
            MonitorService.Log.Error("AlertConditionId was null when creating an alert");
            throw new ArgumentException("AlertConditionId cannot be null");
        }

        var alert = new Alert
        {
            AlertId = Guid.NewGuid(),
            AlertUserId = userId,
            AlertName = dto.AlertName,
            AlertDesc = dto.AlertDesc,
            AlertTime = DateTime.UtcNow,
            AlertPlantConditionId = dto.IsPlantCondition ? dto.AlertConditionId : null,
            AlertDeviceConditionId = !dto.IsPlantCondition ? dto.AlertConditionId : null
        };

        MonitorService.Log.Debug("Creating alert for user " + userId + " with name " + dto.AlertName);

        var savedAlert = await alertRepo.AddAlertAsync(alert);

        string topic = "alerts-" + userId;
        MonitorService.Log.Debug("Broadcasting alert " + alert.AlertId + " to topic " + topic);

        await ws.BroadcastToTopic(topic, new
        {
            type = "alert",
            data = new
            {
                alert.AlertId,
                alert.AlertName,
                alert.AlertDesc,
                alert.AlertTime,
                alert.AlertPlantConditionId,
                alert.AlertDeviceConditionId
            }
        });

        MonitorService.Log.Debug("Successfully created and broadcast alert " + alert.AlertId);

        return savedAlert;
    }

    public async Task<List<AlertResponseDto>> GetAlertsAsync(Guid userId, int? year = null)
    {
        MonitorService.Log.Debug("Entered GetAlertsAsync method in AlertService for user " + userId + " and year " + year);
        return await alertRepo.GetAlertsAsync(userId, year);
    }
}