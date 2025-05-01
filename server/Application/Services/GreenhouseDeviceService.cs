using System.Security.Claims;
using Application.Interfaces;
using Application.Interfaces.Infrastructure.MQTT;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models;
using Application.Models.Dtos;
using Application.Models.Dtos.BroadcastModels;
using Application.Models.Dtos.MqttDtos.Response;
using Application.Models.Dtos.MqttSubscriptionDto;
using Application.Models.Dtos.RestDtos;
using Application.Models.Dtos.RestDtos.SensorHistory;
using Application.Models.Dtos.RestDtos.UserDevice;
using Core.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class GreenhouseDeviceService : IGreenhouseDeviceService
{
    private readonly IOptionsMonitor<AppOptions> _optionsMonitor;
    private readonly ILogger<GreenhouseDeviceService> _logger;
    private readonly IServiceProvider _services;
    private readonly IMqttPublisher _mqttPublisher;
    private readonly IConnectionManager _connectionManager;

    public GreenhouseDeviceService(
        IOptionsMonitor<AppOptions> optionsMonitor,
        ILogger<GreenhouseDeviceService> logger,
        IServiceProvider services,
        IMqttPublisher mqttPublisher,
        IConnectionManager connectionManager)
    {
        _optionsMonitor = optionsMonitor;
        _logger = logger;
        _services = services;
        _mqttPublisher = mqttPublisher;
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

    public async Task<List<GetAllSensorHistoryByDeviceIdDto>> GetSensorHistoryByDeviceIdAndBroadcast(
        Guid deviceId,
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

        return await repo.GetSensorHistoryByDeviceIdAsync(deviceId);
    }
    
    public async Task<GetAllUserDeviceDto> GetAllUserDevice(JwtClaims claims)
    {
        using var scope = _services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IGreenhouseDeviceRepository>();
        
        return await repo.GetAllUserDevices(Guid.Parse(claims.Id));
    }
    
    public async Task<GetRecentSensorDataForAllUserDeviceDto> GetRecentSensorDataForAllUserDevicesAsync(JwtClaims claims)
    {
        using var scope = _services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IGreenhouseDeviceRepository>();
        
        var records = await repo.GetLatestSensorDataForUserDevicesAsync(Guid.Parse(claims.Id));

        return new GetRecentSensorDataForAllUserDeviceDto
        {
            SensorHistoryWithDeviceRecords = records
        };
    }
    
    public Task UpdateDeviceFeed(AdminChangesPreferencesDto dto, JwtClaims claims)
    {
        _mqttPublisher.Publish(
            dto,
            $"{StringConstants.Device}/{dto.DeviceId}/{StringConstants.ChangePreferences}"
        );
        return Task.CompletedTask;
    }

    public async Task DeleteDataAndBroadcast(JwtClaims jwt)
    {
        using var scope = _services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IGreenhouseDeviceRepository>();

        await repo.DeleteAllSensorHistoryData();
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
