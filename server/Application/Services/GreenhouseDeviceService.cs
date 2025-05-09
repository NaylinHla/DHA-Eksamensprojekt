using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models;
using Application.Models.Dtos;
using Application.Models.Dtos.BroadcastModels;
using Application.Models.Dtos.MqttDtos.Response;
using Application.Models.Dtos.MqttSubscriptionDto;
using Application.Models.Dtos.RestDtos.SensorHistory;
using Core.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class GreenhouseDeviceService : IGreenhouseDeviceService
{
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger<GreenhouseDeviceService> _logger;
    private readonly IServiceProvider _services;

    public GreenhouseDeviceService(
        ILogger<GreenhouseDeviceService> logger,
        IServiceProvider services,
        IConnectionManager connectionManager)
    {
        _logger = logger;
        _services = services;
        _connectionManager = connectionManager;
    }

    public async Task AddToDbAndBroadcast(DeviceSensorDataDto? dto)
    {
        if (dto == null)
        {
            _logger.LogWarning("Received null CreateSensorHistoryDto.");
            return;
        }

        var sensorHistory = new SensorHistory
        {
            SensorHistoryId = Guid.NewGuid(),
            DeviceId = Guid.Parse(dto.DeviceId),
            Temperature = dto.Temperature,
            Humidity = dto.Humidity,
            AirPressure = dto.AirPressure,
            AirQuality = dto.AirQuality,
            Time = dto.Time
        };

        // Create a new scope so we get a fresh DbContext/repository
        using var scope = _services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IGreenhouseDeviceRepository>();

        try
        {
            await repo.AddSensorHistory(sensorHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding sensor history");
            return;
        }

        List<GetAllSensorHistoryByDeviceIdDto> recentHistory;
        try
        {
            recentHistory = await repo.GetSensorHistoryByDeviceIdAsync(Guid.Parse(dto.DeviceId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching sensor history");
            return;
        }

        var broadcast = new ServerBroadcastsLiveDataToDashboard
        {
            Logs = recentHistory
        };
        await _connectionManager.BroadcastToTopic(StringConstants.GreenhouseSensorData + "/" + dto.DeviceId, broadcast);
    }

    public async Task<List<GetAllSensorHistoryByDeviceIdDto>> GetSensorHistoryByDeviceId(
        Guid deviceId,
        DateTime? from,
        DateTime? to,
        JwtClaims claims)
    {
        // This call can use the injected repository safely, 
        // since it's part of an HTTP request-scoped call
        var repo = _services.CreateScope()
            .ServiceProvider
            .GetRequiredService<IGreenhouseDeviceRepository>();

        var deviceOwnerId = await repo.GetDeviceOwnerUserId(deviceId);
        if (deviceOwnerId != Guid.Parse(claims.Id))
            throw new UnauthorizedAccessException("You do not own this device.");

        return await repo.GetSensorHistoryByDeviceIdAsync(deviceId, from, to);
    }

    public async Task<GetRecentSensorDataForAllUserDeviceDto> GetRecentSensorDataForAllUserDevicesAsync(
        JwtClaims claims)
    {
        using var scope = _services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IGreenhouseDeviceRepository>();

        var records = await repo.GetLatestSensorDataForUserDevicesAsync(Guid.Parse(claims.Id));

        return new GetRecentSensorDataForAllUserDeviceDto
        {
            SensorHistoryWithDeviceRecords = records
        };
    }

    public async Task DeleteDataFromSpecificDeviceAndBroadcast(Guid deviceId, JwtClaims claims)
    {
        var repo = _services.CreateScope()
            .ServiceProvider
            .GetRequiredService<IGreenhouseDeviceRepository>();

        var deviceOwnerId = await repo.GetDeviceOwnerUserId(deviceId);
        if (deviceOwnerId != Guid.Parse(claims.Id))
            throw new UnauthorizedAccessException("You do not own this device.");

        await repo.DeleteDataFromSpecificDevice(deviceId);
        await _connectionManager.BroadcastToTopic(
            StringConstants.Dashboard,
            new AdminHasDeletedData()
        );
    }
}

public class AdminHasDeletedData : ApplicationBaseDto
{
    public override string eventType { get; set; } = nameof(AdminHasDeletedData);
}