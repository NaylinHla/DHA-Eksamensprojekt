using Api.Rest.Extensions;
using Application.Interfaces;
using Application.Models.Dtos.RestDtos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest.Controllers;

[ApiController]
public class AuthController(ISecurityService securityService) : ControllerBase
{
    private const string ControllerRoute = "api/auth/";

    public const string LoginRoute = ControllerRoute + nameof(Login);


    public const string RegisterRoute = ControllerRoute + nameof(Register);


    public const string SecuredRoute = ControllerRoute + nameof(Secured);


    [HttpPost]
    [Route(LoginRoute)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] AuthLoginDto dto)
    {
        return Ok(await securityService.Login(dto));
    }

    [Route(RegisterRoute)]
    [HttpPost]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] AuthRegisterDto dto)
    {
        return Ok(await securityService.Register(dto));
    }

    [HttpGet]
    [Route(SecuredRoute)]
    public ActionResult Secured()
    {
        securityService.VerifyJwtOrThrow(HttpContext.GetJwt());
        return Ok("You are authorized to see this message");
    }
}