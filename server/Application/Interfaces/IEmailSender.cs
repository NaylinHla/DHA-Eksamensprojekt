using Application.Models.Dtos.RestDtos.EmailList.Request;

public interface IEmailSender
{
    Task SendEmailAsync(string subject, string message);
    Task AddEmailAsync(AddEmailDto dto);
    Task RemoveEmailAsync(RemoveEmailDto dto);
}