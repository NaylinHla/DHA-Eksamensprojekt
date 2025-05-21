using System.ComponentModel.DataAnnotations;
using Application.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class AppOptionsExtensions
{
    public static AppOptions AddAppOptions(this IServiceCollection services, IConfiguration configuration)
    {
        var appOptions = new AppOptions();
        configuration.GetSection(nameof(AppOptions)).Bind(appOptions);

        services.Configure<AppOptions>(configuration.GetSection(nameof(AppOptions)));

        ICollection<ValidationResult> results = new List<ValidationResult>();
        var validated = Validator.TryValidateObject(appOptions, new ValidationContext(appOptions), results, true);
        if (!validated)
            throw new MissingFieldException($"{string.Join(", ", results.Select(r => r.ErrorMessage))}");

        return appOptions;
    }
}