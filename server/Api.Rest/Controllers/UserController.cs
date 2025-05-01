using System.Security.Authentication;
using Application.Interfaces;
using Application.Models.Dtos.RestDtos.Request;
using Application.Services;
using Core.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService userService, ISecurityService securityService) : ControllerBase
{
    public const string DeleteUserRoute = nameof(DeleteUser);

    [HttpPost]
    [Route(DeleteUserRoute)]
    public ActionResult<User> DeleteUser([FromHeader] string authorization, [FromBody] DeleteUserDto dto)
    {
        try
        {
            // Verify JWT and extract user claims (e.g., Email)
            var claims = securityService.VerifyJwtOrThrow(authorization);

            // Create request DTO using email from claims
            var request = new DeleteUserDto
            {
                Email = claims.Email
            };

            // Perform deletion and return result
            var deletedUser = userService.DeleteUser(request);
            return Ok(deletedUser);
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new { message = e.Message });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "Der opstod en fejl", detail = e.Message });
        }
    }
    
    [HttpPatch]
    [Route("email")]
    public ActionResult<User> PatchUserEmail([FromHeader] string authorization, [FromBody] PatchUserEmailDto dto)
    {
        try
        {
            // Verify JWT and extract user claims (e.g., Email)
            var claims = securityService.VerifyJwtOrThrow(authorization);

            // Create request DTO using email from claims
            var request = new PatchUserEmailDto
            {
                OldEmail = claims.Email,
                NewEmail = dto.NewEmail
            };

            // Perform email update and return result
            var updatedUser = userService.PatchUserEmail(request);
            return Ok(updatedUser);
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new { message = e.Message });
        }
        catch (ArgumentException e)
        {
            return BadRequest(new { message = e.Message });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "Der opstod en fejl", detail = e.Message });
        }
    }
    
    [HttpPatch]
    [Route("password")]
    public ActionResult<User> PatchUserPassword([FromHeader] string authorization, [FromBody] PatchUserPasswordDto dto)
    {
        try
        {
            var claims = securityService.VerifyJwtOrThrow(authorization);
            var updatedUser = userService.PatchUserPassword(claims.Email, dto);
            return Ok(updatedUser);
        }
        catch (AuthenticationException e)
        {
            return Unauthorized(new { message = e.Message });
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new { message = e.Message });
        }
        catch (ArgumentException e)
        {
            return BadRequest(new { message = e.Message });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { message = "Der opstod en fejl", detail = e.Message });
        }
    }

}