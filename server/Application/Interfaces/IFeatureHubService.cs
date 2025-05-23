namespace Application.Interfaces
{
    public interface IFeatureHubService
    {
        Task<bool> IsFeatureEnabledAsync(string featureKey);
    }
}