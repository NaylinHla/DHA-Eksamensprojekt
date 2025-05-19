using Application.Interfaces;
using Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class Extensions
{
    public static IServiceCollection RegisterApplicationServices(this IServiceCollection services)
    {
        // Add Services
        services.AddScoped<ISecurityService, SecurityService>();
        services.AddScoped<IGreenhouseDeviceService, GreenhouseDeviceService>();
        services.AddScoped<IWebsocketSubscriptionService, WebsocketSubscriptionService>();
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPlantService, PlantService>();
        services.AddScoped<IUserDeviceService, UserDeviceService>();
        services.AddScoped<IUserSettingsService, UserSettingsService>();
        services.AddScoped<IAlertConditionService, AlertConditionService>();
        
        // Add Validators
            // Alert
        services.AddValidatorsFromAssemblyContaining<Validation.Alert.AlertCreateDtoValidator>();
            // Auth
        services.AddValidatorsFromAssemblyContaining<Validation.Auth.AuthLoginDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<Validation.Auth.AuthRegisterDtoValidator>();
            // Email
        services.AddValidatorsFromAssemblyContaining<Validation.Email.AddEmailDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<Validation.Email.RemoveEmailDtoValidator>();
            // MQTT
        services.AddValidatorsFromAssemblyContaining<Validation.MQTT.AdminChangesPreferencesDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<Validation.MQTT.DeviceSensorDataDtoValidator>();
            // Plant
        services.AddValidatorsFromAssemblyContaining<Validation.Plant.PlantCreateDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<Validation.Plant.PlantEditDtoValidator>();
            // User
        services.AddValidatorsFromAssemblyContaining<Validation.User.DeleteUserDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<Validation.User.PatchUserEmailDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<Validation.User.PatchUserPasswordDtoValidator>();
            // UserDevice
        services.AddValidatorsFromAssemblyContaining<Validation.UserDevice.UserDeviceCreateDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<Validation.UserDevice.UserDeviceEditDtoValidator>();    
        
        return services;
    }
}