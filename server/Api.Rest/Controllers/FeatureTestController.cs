using FeatureHubSDK;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeatureTestController(IClientContext featureHubContext) : ControllerBase
{
    [HttpGet("toggle")]
    public IActionResult CheckToggle()
    {
        var isEnabled = featureHubContext["FeatureToggleTest"].IsEnabled;
        return Ok(new
        {
            Feature = "FeatureToggleTest",
            Enabled = isEnabled
        });
    }
}