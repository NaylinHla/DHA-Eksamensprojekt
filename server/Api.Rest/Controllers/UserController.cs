using Application.Interfaces;
using Application.Models.Dtos.RestDtos.Request;
using Core.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService userService, ISecurityService securityService) : ControllerBase
{
    public const string DeleteUserRoute = nameof(DeleteUser);
    public const string PatchUserEmailRoute = nameof(PatchUserEmail);
    public const string PatchUserPasswordRoute = nameof(PatchUserPassword);
    public const string GetUserRoute = nameof(GetUser);

    [HttpGet]
    [Route(GetUserRoute)]
    public async Task<ActionResult<User>> GetUser(
        [FromHeader] string authorization)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);
        var user = await userService.GetUserByEmailAsync(claims.Email);
        return Ok(user);
    }
    
    [HttpDelete]
    [Route(DeleteUserRoute)]
    public async Task<ActionResult<User>> DeleteUser(
        [FromHeader] string authorization)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);
        await userService.DeleteUser(new DeleteUserDto { Email = claims.Email });
        return Ok();
    }

    [HttpPatch]
    [Route(PatchUserEmailRoute)]
    public async Task<ActionResult<User>> PatchUserEmail(
        [FromHeader] string authorization, 
        [FromBody] PatchUserEmailDto dto)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);

        var request = new PatchUserEmailDto
        {
            OldEmail = claims.Email,
            NewEmail = dto.NewEmail
        };
        var updatedUser = await userService.PatchUserEmail(request);
        return Ok(updatedUser);
    }

    [HttpPatch]
    [Route(PatchUserPasswordRoute)]
    public async Task<ActionResult<User>> PatchUserPassword(
        [FromHeader] string authorization, 
        [FromBody] PatchUserPasswordDto dto)
    {
        var claims = securityService.VerifyJwtOrThrow(authorization);
        var updatedUser = await userService.PatchUserPassword(claims.Email, dto);
        return Ok(updatedUser);
    }
}