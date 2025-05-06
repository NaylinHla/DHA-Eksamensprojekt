using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;
using Application.Models.Dtos.RestDtos.EmailList.Request;
using Core.Domain.Entities;

namespace Api.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailController : ControllerBase
{
    private readonly IEmailSender _emailSender;

    public EmailController(IEmailSender emailSender)
    {
        _emailSender = emailSender;
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

}