using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models.Dtos.BroadcastModels;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;
using FluentValidation;
using Infrastructure.Logging;

namespace Application.Services;

public class AlertService(
    IAlertRepository alertRepo,
    IAlertConditionRepository alertConditionRepo,
    IPlantRepository plantRepo,
    IUserDeviceRepository userDeviceRepo,
    IConnectionManager ws,
    IValidator<AlertCreateDto> alertCreateValidator, IValidator<IsAlertUserDeviceConditionMeetDto> isAlertUserDeviceConditionMeetValidator) : IAlertService

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

        MonitorService.Log.Debug("Creating alert for user {UserId} with name {AlertName}", userId, dto.AlertName);

        var savedAlert = await alertRepo.AddAlertAsync(alert);

        var topic = $"alerts-{userId}";
        MonitorService.Log.Debug("Broadcasting alert {AlertId} to topic {Topic}", alert.AlertId, topic);

        var alertDto = new AlertDto
        {
            AlertId = alert.AlertId.ToString(),
            AlertName = alert.AlertName,
            AlertDesc = alert.AlertDesc,
            AlertTime = alert.AlertTime,
            AlertPlantConditionId = alert.AlertPlantConditionId.ToString(),
            AlertDeviceConditionId = alert.AlertDeviceConditionId.ToString()
        };

        var broadcast = new ServerBroadcastsLiveAlertToAlertView
        {
            Alerts = [alertDto]
        };

        await ws.BroadcastToTopic($"alerts-{userId}", broadcast);

        MonitorService.Log.Debug("Successfully created and broadcast alert {AlertId} ", alert.AlertId);

        return savedAlert;
    }

    public async Task<List<AlertResponseDto>> GetAlertsAsync(Guid userId, int? year = null)
    {
        MonitorService.Log.Debug("Entered GetAlertsAsync method in AlertService for user {UserId} and year {Year}",
            userId, year);
        return await alertRepo.GetAlertsAsync(userId, year);
    }

    public async Task CheckAndTriggerScheduledPlantAlertsAsync()
    {
        MonitorService.Log.Debug("Running scheduled check for condition alerts");

        var alertsToSend = await alertConditionRepo.GetAllConditionAlertPlantForAllUserAsync();

        foreach (var alert in alertsToSend)
        {
            var plant = await plantRepo.GetPlantByIdAsync(alert.PlantId);
            if (plant == null)
            {
                MonitorService.Log.Warning("Plant not found for ID: {PlantId}", alert.PlantId);
                continue;
            }

            var userId = await plantRepo.GetPlantOwnerUserId(alert.PlantId);
            if (userId == Guid.Empty)
            {
                MonitorService.Log.Warning("User not found for PlantId: {PlantId}", alert.PlantId);
                continue;
            }
            
            if (plant.WaterEvery.HasValue)
            {
                var last = plant.LastWatered ?? plant.Planted ?? DateTime.MinValue;
                // next due = last + interval days
                var nextDue = last.AddDays(plant.WaterEvery.Value);
                if (DateTime.UtcNow < nextDue)
                {
                    continue;
                }
            }
            
            var dto = new AlertCreateDto
            {
                AlertName = $"Scheduled Water Alert for {plant.PlantName}",
                AlertDesc = $"Reminder: Check water conditions for {plant.PlantName}",
                AlertConditionId = alert.ConditionAlertPlantId,
                IsPlantCondition = true,
                AlertUser = userId
            };

            await CreateAlertAsync(userId, dto);
        }

        MonitorService.Log.Debug("Finished scheduled plant alert check");
    }

    public async Task TriggerUserDeviceConditionAsync(IsAlertUserDeviceConditionMeetDto dto)
    {
        MonitorService.Log.Debug("Running scheduled check for user-device conditions");

        await isAlertUserDeviceConditionMeetValidator.ValidateAndThrowAsync(dto);
        
        var matched = await alertConditionRepo.IsAlertUserDeviceConditionMeet(dto);
        if (matched.Count == 0)
        {
            MonitorService.Log.Debug("No conditions met for UserDeviceId: {UserDeviceId}", dto.UserDeviceId);
            return;
        }

        if (!Guid.TryParse(dto.UserDeviceId, out var deviceId) ||
            (await userDeviceRepo.GetUserDeviceByIdAsync(deviceId)) is not { } userDevice)
        {
            MonitorService.Log.Error("Invalid or missing UserDevice for ID: {UserDeviceId}", dto.UserDeviceId);
            return;
        }

        foreach (var idStr in matched)
        {
            if (!Guid.TryParse(idStr, out var conditionId))
            {
                MonitorService.Log.Error("Invalid ConditionId: {ConditionId}", idStr);
                continue;
            }

            var condition = await alertConditionRepo
                .GetConditionAlertUserDeviceIdByConditionAlertIdAsync(conditionId);
            if (condition == null)
            {
                MonitorService.Log.Error("Condition not found for ID: {ConditionId}", conditionId);
                continue;
            }

            var sensorType = condition.SensorType;
            var rawValue = dto.GetType()
                .GetProperty(sensorType)?
                .GetValue(dto)?
                .ToString();
            if (string.IsNullOrEmpty(rawValue))
            {
                MonitorService.Log.Error("Missing value for sensor '{SensorType}'", sensorType);
                continue;
            }

            var (_, desc) = sensorType switch
            {
                "Temperature" => ("°C", $"The temperature is {rawValue}°C, meeting {condition.Condition}"),
                "Humidity" => ("%", $"Humidity is {rawValue}%, triggering {condition.Condition}"),
                "AirPressure" => ("hPa", $"Air pressure is {rawValue}hPa, meeting {condition.Condition}"),
                "AirQuality" => ("ppm", $"Air quality index is {rawValue}ppm, hitting {condition.Condition}"),
                _ => ("", $"Sensor '{sensorType}' = {rawValue}, matched {condition.Condition}")
            };

            var createDto = new AlertCreateDto
            {
                AlertName = $"Alert: {sensorType} threshold on {userDevice.DeviceName}",
                AlertDesc = desc,
                AlertConditionId = conditionId,
                IsPlantCondition = false,
                AlertUser = userDevice.UserId
            };

            await CreateAlertAsync(userDevice.UserId, createDto);
            MonitorService.Log.Debug(
                "Created alert for DeviceId {DeviceId}, ConditionId {ConditionId}",
                dto.UserDeviceId, conditionId);
        }

        MonitorService.Log.Debug(
            "Total of {Count} alert(s) sent for UserDeviceId: {UserDeviceId}",
            matched.Count, dto.UserDeviceId);
    }
}