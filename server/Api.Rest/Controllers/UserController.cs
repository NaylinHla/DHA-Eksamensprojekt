using Application.Models.Dtos.RestDtos.Request;
using Application.Services;
using Core.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpDelete]
    [Route("delete")]
    public ActionResult<User> DeleteUser([FromBody] DeleteUserDto request)
    {
        try
        {
            var deletedUser = _userService.DeleteUser(request);
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