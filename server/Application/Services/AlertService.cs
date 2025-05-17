using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;
using FluentValidation;
using Infrastructure.Logging;

namespace Application.Services;

public class AlertService(
    IAlertRepository alertRepo, 
    IConnectionManager ws,
    IValidator<AlertCreateDto> alertCreateValidator) : IAlertService
{
    public async Task<Alert> CreateAlertAsync(Guid userId, AlertCreateDto dto)
    {
        MonitorService.Log.Debug("Entered CreateAlertAsync method in AlertService");
        
        await alertCreateValidator.ValidateAndThrowAsync(dto);

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

        MonitorService.Log.Debug("Creating alert for user {UserId} with name {AlertName}", userId,dto.AlertName);

        var savedAlert = await alertRepo.AddAlertAsync(alert);

        var topic = "alerts-" + userId;
        MonitorService.Log.Debug("Broadcasting alert {AlertId} to topic {Topic} ", alert.AlertId ,topic);

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

        MonitorService.Log.Debug("Successfully created and broadcast alert {AlertId} ", alert.AlertId);

        return savedAlert;
    }

    public async Task<List<AlertResponseDto>> GetAlertsAsync(Guid userId, int? year = null)
    {
        MonitorService.Log.Debug("Entered GetAlertsAsync method in AlertService for user: {UserId} and year: {Year} ", userId, year);
        return await alertRepo.GetAlertsAsync(userId, year);
    }
}