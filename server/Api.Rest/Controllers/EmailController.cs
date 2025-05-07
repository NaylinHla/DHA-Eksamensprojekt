using Application.Interfaces;
using Application.Models.Dtos.RestDtos.EmailList.Request;
using Application.Services;
using Core.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailController : ControllerBase
{
    private readonly IEmailSender _emailSender;
    private readonly JwtEmailTokenService _jwtService;

    public EmailController(IEmailSender emailSender, JwtEmailTokenService jwtService)
    {
        _emailSender = emailSender;
        _jwtService = jwtService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
    {
        await _emailSender.SendEmailAsync(request.Subject, request.Message);
        return Ok("Email sent successfully.");
    }

    [HttpPost("subscribe")]
    public async Task<IActionResult> SubscribeToEmailList([FromBody] AddEmailDto dto)
    {
        await _emailSender.AddEmailAsync(dto);
        return Ok("Subscription confirmed and email sent.");
    }

    [HttpPost("unsubscribe")]
    public async Task<IActionResult> UnsubscribeFromEmailList([FromBody] RemoveEmailDto dto)
    {
        await _emailSender.RemoveEmailAsync(dto);
        return Ok("Unsubscription confirmed and email sent.");
    }

    [HttpGet("unsubscribe")]
    public async Task<IActionResult> UnsubscribeFromEmailLink([FromQuery] string token)
    {
        var email = _jwtService.ValidateToken(token);
        if (email == null)
            return BadRequest("Invalid or expired token.");

        await _emailSender.RemoveEmailAsync(new RemoveEmailDto { Email = email });
        return Ok("You have been unsubscribed.");
    }
}