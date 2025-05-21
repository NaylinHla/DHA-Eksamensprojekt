using Application.Interfaces;
using FeatureHubSDK;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Application.Models;

namespace Application.Services
{
    public class FeatureHubService : IFeatureHubService
    {
        private readonly IClientContext _context;

        public FeatureHubService(IClientContext context)
        {
            _context = context;
        }

        public async Task<bool> IsFeatureEnabledAsync(string featureKey)
        {
            await _context.Build(); // Ensure the context is ready
            return _context[featureKey]?.IsEnabled ?? false;
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
                var context = config.NewContext();
                context.Build().Wait(); // sync startup
                return context;
            });

            services.AddSingleton<IFeatureHubService, FeatureHubService>();
        }
    }
}