﻿using Application.Interfaces;
using Application.Models.Dtos.RestDtos.EmailList.Request;
using Application.Services;
using Core.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailController(IEmailSender emailSender, JwtEmailTokenService jwtService) : ControllerBase
{
    
    public const string SendEmailRoute = nameof(SendEmail);
    public const string SubscribeToEmailListRoute = nameof(SubscribeToEmailList);
    public const string UnsubscribeFromEmailListRoute = nameof(UnsubscribeFromEmailList);
    public const string UnsubscribeFromEmailLinkRoute = nameof(UnsubscribeFromEmailLink);
    
    [HttpPost]
    [Route(SendEmailRoute)]
    public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
    {
        await emailSender.SendEmailAsync(request.Subject, request.Message);
        return Ok("Email sent successfully.");
    }

    [HttpPost]
    [Route(SubscribeToEmailListRoute)]
    public async Task<IActionResult> SubscribeToEmailList([FromBody] AddEmailDto dto)
    {
        await emailSender.AddEmailAsync(dto);
        return Ok("Subscription confirmed and email sent.");
    }

    [HttpPost]
    [Route(UnsubscribeFromEmailListRoute)]
    public async Task<IActionResult> UnsubscribeFromEmailList([FromBody] RemoveEmailDto dto)
    {
        await emailSender.RemoveEmailAsync(dto);
        return Ok("Unsubscription confirmed and email sent.");
    }

    [HttpGet]
    [Route(UnsubscribeFromEmailLinkRoute)]
    public async Task<IActionResult> UnsubscribeFromEmailLink([FromQuery] string token)
    {
        var email = jwtService.ValidateToken(token);
        if (email == null)
            return BadRequest("Invalid or expired token.");

        await emailSender.RemoveEmailAsync(new RemoveEmailDto { Email = email });
        return Ok("You have been unsubscribed.");
    }
}