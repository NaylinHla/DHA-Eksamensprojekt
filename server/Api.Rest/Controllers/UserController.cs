using Application.Interfaces;
using Application.Models.Dtos.RestDtos.Request;
using Application.Services;
using Core.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(UserService userService, ISecurityService securityService) : ControllerBase
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
}