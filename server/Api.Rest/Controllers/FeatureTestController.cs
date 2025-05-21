using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeatureTestController(IFeatureHubService featureHubService) : ControllerBase
{
    [HttpGet("toggle")]
    public async Task<IActionResult> CheckToggle()
    {
        var isEnabled = await featureHubService.IsFeatureEnabledAsync("FeatureToggleTest");

        return Ok(new
        {
            Feature = "FeatureToggleTest",
            Enabled = isEnabled
        });
    }
}