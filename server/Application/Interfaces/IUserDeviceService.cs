using Application.Models;
using Application.Models.Dtos.RestDtos;
using Application.Models.Dtos.RestDtos.UserDevice.Request;
using Core.Domain.Entities;

namespace Application.Interfaces;

public interface IUserDeviceService
{
    Task<UserDevice?> GetUserDeviceAsync(Guid deviceId, JwtClaims claims);
    Task<List<UserDevice>> GetAllUserDeviceAsync(JwtClaims claims);
    Task<UserDevice> CreateUserDeviceAsync(UserDeviceCreateDto dto, JwtClaims claims);
    Task<UserDevice> UpdateUserDeviceAsync(Guid deviceId, UserDeviceEditDto dto, JwtClaims claims);
    Task DeleteUserDeviceAsync(Guid deviceId, JwtClaims claims);
    Task UpdateDeviceFeed(AdminChangesPreferencesDto dto, JwtClaims claims);
}