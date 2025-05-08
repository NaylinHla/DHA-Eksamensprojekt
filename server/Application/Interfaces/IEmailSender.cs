using Application.Models.Dtos.RestDtos.EmailList.Request;

namespace Application.Interfaces;
public interface IEmailSender
{
    Task SendEmailAsync(string subject, string message);
    Task AddEmailAsync(AddEmailDto dto);
    Task RemoveEmailAsync(RemoveEmailDto dto);
}