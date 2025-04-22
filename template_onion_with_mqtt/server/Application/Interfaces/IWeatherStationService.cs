using Application.Models;
using Application.Models.Dtos.MqttSubscriptionDto;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;

namespace Application.Interfaces;

public interface IWeatherStationService
{
    List<Devicelog> GetDeviceFeed(JwtClaims client);
    Task AddToDbAndBroadcast(DeviceLogDto? dto);
    Task UpdateDeviceFeed(AdminChangesPreferencesDto dto, JwtClaims claims);
    Task DeleteDataAndBroadcast(JwtClaims jwt);
}