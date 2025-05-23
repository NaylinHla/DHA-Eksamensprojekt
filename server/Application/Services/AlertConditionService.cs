using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Models;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Exceptions;
using FluentValidation;
using Infrastructure.Logging;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace Application.Services
{
    public class AlertConditionService(
        IAlertConditionRepository alertConditionRepo,
        IPlantRepository plantRepository,
        IUserDeviceRepository userDeviceRepository, 
        IValidator<ConditionAlertUserDeviceCreateDto> conditionAlertUserDeviceCreateValidator,
        IValidator<ConditionAlertUserDeviceEditDto> conditionAlertUserDeviceEditValidator,
        IValidator<ConditionAlertPlantCreateDto> conditionAlertPlantCreateValidator): IAlertConditionService
    {
        private const string AlertConditionNotFound = "Alert Condition not found.";
        private const string UnauthorizedAlertConditionAccess = "You do not own this alert condition.";

        public async Task<ConditionAlertPlantResponseDto?> GetConditionAlertPlantByIdAsync(Guid plantId,
            JwtClaims claims)
        {
            var plantOwnerId = await plantRepository.GetPlantOwnerUserId(plantId);
            if (plantOwnerId != Guid.Parse(claims.Id))
            {
                MonitorService.Log.Debug(
                    "Unauthorized access attempt for Alert Condition Plant with PlantId: {PlantId} by UserId: {UserId}",
                    plantId, claims.Id);
                throw new UnauthorizedAccessException(UnauthorizedAlertConditionAccess);
            }
            
            MonitorService.Log.Debug("Fetching PlantId: {PlantId}´Alert Condition", plantId);
            var conditionAlertPlant = await alertConditionRepo.GetConditionAlertPlantByIdAsync(plantId)
                                      ?? throw new NotFoundException(AlertConditionNotFound);

            MonitorService.Log.Debug("Fetched Alert Condition Plant with PlantId: {PlantId} successfully", plantId);
            return conditionAlertPlant;
        }

        public async Task<List<ConditionAlertPlantResponseDto>> GetAllConditionAlertPlantsAsync(Guid userId,
            JwtClaims claims)
        {
            if (userId != Guid.Parse(claims.Id))
            {
                MonitorService.Log.Debug(
                    "Unauthorized access attempt for Get All Alert Condition Plant by UserId: {UserId}", claims.Id);
                throw new UnauthorizedAccessException(UnauthorizedAlertConditionAccess);
            }

            MonitorService.Log.Debug("Fetching all alert condition plants for UserId: {UserId}", userId);

            var plants = await alertConditionRepo.GetAllConditionAlertPlantsAsync(userId);
            MonitorService.Log.Debug("Fetched {Count} alert condition plants for UserId: {UserId}", plants.Count,
                userId);
            return plants;
        }

        public async Task<ConditionAlertPlantResponseDto> CreateConditionAlertPlantAsync(
            ConditionAlertPlantCreateDto dto,
            JwtClaims claims)
        {
            
            await conditionAlertPlantCreateValidator.ValidateAndThrowAsync(dto);
            
            MonitorService.Log.Debug(
                "Creating new alert condition plant with ConditionAlertPlantId: {ConditionAlertPlantId}",
                dto.PlantId);

            if (dto.PlantId == Guid.Empty)
            {
                throw new ValidationException("The PlantId is required.");
            }

            var plant = await plantRepository.GetPlantByIdAsync(dto.PlantId);
            if (plant == null)
            {
                throw new NotFoundException(AlertConditionNotFound);
            }

            var ownerId = await plantRepository.GetPlantOwnerUserId(plant.PlantId);
            if (ownerId != Guid.Parse(claims.Id))
            {
                throw new UnauthorizedAccessException(UnauthorizedAlertConditionAccess);
            }

            var createdAlertCondition = await alertConditionRepo.AddConditionAlertPlantAsync(plant.PlantId);
            MonitorService.Log.Debug(
                "Created new Alert Condition Plant with ConditionAlertPlantId: {ConditionAlertPlantId}",
                createdAlertCondition.PlantId);

            return createdAlertCondition;
        }

        public async Task DeleteConditionAlertPlantAsync(Guid conditionAlertPlantId, JwtClaims claims)
        {
            MonitorService.Log.Debug(
                "Deleting Alert Condition Plant with ConditionAlertPlantId: {ConditionAlertPlantId}",
                conditionAlertPlantId);
            
            var conditionAlertPlant = await alertConditionRepo
                                          .GetConditionAlertPlantIdByConditionAlertIdAsync(conditionAlertPlantId)
                                      ?? throw new NotFoundException(AlertConditionNotFound);

            // Check ownership via the foreign key PlantId
            var plantOwnerId = await plantRepository.GetPlantOwnerUserId(conditionAlertPlant.PlantId);
            if (plantOwnerId != Guid.Parse(claims.Id))
            {
                MonitorService.Log.Debug(
                    "Unauthorized delete attempt for Alert Condition Plant with ConditionAlertPlantId: {ConditionAlertPlantId} by UserId: {UserId}",
                    conditionAlertPlantId, claims.Id);
                throw new UnauthorizedAccessException(UnauthorizedAlertConditionAccess);
            }

            await alertConditionRepo.DeleteConditionAlertPlantAsync(conditionAlertPlantId);

            MonitorService.Log.Debug(
                "Deleted Alert Condition Plant with ConditionAlertPlantId: {ConditionAlertPlantId}",
                conditionAlertPlantId);
        }

        public async Task<List<ConditionAlertUserDeviceResponseDto>> GetConditionAlertUserDeviceByIdAsync(
            Guid userDeviceId, JwtClaims claims)
        {
            var deviceOwnerId =
                await userDeviceRepository.GetUserDeviceOwnerUserIdAsync(userDeviceId);
            if (deviceOwnerId != Guid.Parse(claims.Id))
            {
                MonitorService.Log.Debug(
                    "Unauthorized access attempt for Alert Condition User Device with UserDeviceId: {ConditionAlertUserDeviceId} by UserId: {UserId}",
                    userDeviceId, claims.Id);
                throw new UnauthorizedAccessException(UnauthorizedAlertConditionAccess);
            }
            
            MonitorService.Log.Debug("Fetching Alert Condition User Device with UserDeviceId: {ConditionAlertUserDeviceId}", userDeviceId);

            var conditionAlertUserDevice =
                await alertConditionRepo.GetConditionsAlertUserDeviceByIdAsync(userDeviceId)
                ?? throw new NotFoundException(AlertConditionNotFound);


            MonitorService.Log.Debug("Fetched Alert Condition User Device with UserDeviceId: {UserDeviceId} successfully", userDeviceId);
            return conditionAlertUserDevice;
        }

        public async Task<List<ConditionAlertUserDeviceResponseDto>> GetAllConditionAlertUserDevicesAsync(Guid userId,
            JwtClaims claims)
        {
            if (userId != Guid.Parse(claims.Id))
            {
                MonitorService.Log.Debug(
                    "Unauthorized access attempt for Get All Alert Condition UserDevice by UserId: {UserId}",
                    claims.Id);
                throw new UnauthorizedAccessException(UnauthorizedAlertConditionAccess);
            }

            MonitorService.Log.Debug("Fetching all alert condition user devices for UserId: {UserId}", userId);

            var devices = await alertConditionRepo.GetAllConditionAlertUserDevicesAsync(userId);
            MonitorService.Log.Debug("Fetched {Count} alert condition user devices for UserId: {UserId}", devices.Count,
                userId);
            return devices;
        }

        public async Task<ConditionAlertUserDeviceResponseDto> CreateConditionAlertUserDeviceAsync(
            ConditionAlertUserDeviceCreateDto dto, JwtClaims claims)
        {
            MonitorService.Log.Debug("Creating new alert condition user device");

            await conditionAlertUserDeviceCreateValidator.ValidateAndThrowAsync(dto);
            
            var deviceOwnerId = await userDeviceRepository.GetUserDeviceOwnerUserIdAsync(Guid.Parse(dto.UserDeviceId));
            if (deviceOwnerId != Guid.Parse(claims.Id))
            {
                throw new UnauthorizedAccessException(UnauthorizedAlertConditionAccess);
            }

            var createdDevice = await alertConditionRepo.AddConditionAlertUserDeviceAsync(dto);
            MonitorService.Log.Debug(
                "Created new Alert Condition User Device with ConditionAlertUserDeviceId: {ConditionAlertUserDeviceId}",
                createdDevice.UserDeviceId);

            return createdDevice;
        }

        public async Task<ConditionAlertUserDeviceResponseDto> EditConditionAlertUserDeviceAsync(
            ConditionAlertUserDeviceEditDto dto, JwtClaims claims)
        {
            MonitorService.Log.Debug(
                "Editing Alert Condition User Device with ConditionAlertUserDeviceId: {UserDeviceId}",
                dto.UserDeviceId);

            await conditionAlertUserDeviceEditValidator.ValidateAndThrowAsync(dto);
            
            var deviceOwnerId = await userDeviceRepository.GetUserDeviceOwnerUserIdAsync(Guid.Parse(dto.UserDeviceId));
            if (deviceOwnerId != Guid.Parse(claims.Id))
            {
                throw new UnauthorizedAccessException(UnauthorizedAlertConditionAccess);
            }

            var updatedDevice = await alertConditionRepo.EditConditionAlertUserDeviceAsync(dto);
            MonitorService.Log.Debug(
                "Updated Alert Condition User Device with ConditionAlertUserDeviceId: {ConditionAlertUserDeviceId}",
                updatedDevice.ConditionAlertUserDeviceId);

            return updatedDevice;
        }

        public async Task DeleteConditionAlertUserDeviceAsync(Guid conditionAlertUserDeviceId, JwtClaims claims)
        {
            MonitorService.Log.Debug(
                "Deleting Alert Condition User Device with ConditionAlertUserDeviceId: {ConditionAlertUserDeviceId}",
                conditionAlertUserDeviceId);
            
            var conditionAlertUserDevice =
                await alertConditionRepo.GetConditionAlertUserDeviceIdByConditionAlertIdAsync(conditionAlertUserDeviceId)
                ?? throw new NotFoundException(AlertConditionNotFound);

            // Ownership check
            var deviceOwnerId =
                await userDeviceRepository.GetUserDeviceOwnerUserIdAsync(conditionAlertUserDevice.UserDeviceId);
            if (deviceOwnerId != Guid.Parse(claims.Id))
            {
                MonitorService.Log.Debug(
                    "Unauthorized delete attempt for Alert Condition User Device with ConditionAlertUserDeviceId: {ConditionAlertUserDeviceId} by UserId: {UserId}",
                    conditionAlertUserDeviceId, claims.Id);
                throw new UnauthorizedAccessException(UnauthorizedAlertConditionAccess);
            }

            await alertConditionRepo.DeleteConditionAlertUserDeviceAsync(conditionAlertUserDeviceId);
            MonitorService.Log.Debug(
                "Deleted Alert Condition User Device with ConditionAlertUserDeviceId: {ConditionAlertUserDeviceId}",
                conditionAlertUserDeviceId);
        }
    }
}