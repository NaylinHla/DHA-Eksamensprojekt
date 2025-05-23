using Application.Interfaces;
using FeatureHubSDK;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Application.Models;

namespace Application.Services
{
    public class FeatureHubService(IClientContext context) : IFeatureHubService
    {
        public async Task<bool> IsFeatureEnabledAsync(string featureKey)
        {
            await context.Build(); // Ensure the context is ready
            return context[featureKey]?.IsEnabled ?? false;
        }

        public static void AddFeatureHub(IServiceCollection services)
        {
            services.AddSingleton<EdgeFeatureHubConfig>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<AppOptions>>().Value;

                var apiKey = options.ApiKey;
                var edgeUrl = options.EdgeUrl;

                if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(edgeUrl))
                    throw new FeatureHubKeyInvalidException("Edge URL or SDK key is missing from configuration.");

                return new EdgeFeatureHubConfig(edgeUrl, apiKey);
            });

            services.AddSingleton<IClientContext>(provider =>
            {
                var config = provider.GetRequiredService<EdgeFeatureHubConfig>();
                var clientContext = config.NewContext();
                clientContext.Build().Wait(); // sync startup
                return clientContext;
            });

            services.AddSingleton<IFeatureHubService, FeatureHubService>();
        }
    }
}