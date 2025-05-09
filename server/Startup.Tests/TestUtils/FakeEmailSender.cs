using System.Threading.Tasks;
using Application.Interfaces;
using Application.Models.Dtos.RestDtos.EmailList.Request;

namespace Startup.Tests.TestUtils;

public class FakeEmailSender : IEmailSender
{
    public Task AddEmailAsync(AddEmailDto dto) => Task.CompletedTask;

    public Task RemoveEmailAsync(RemoveEmailDto dto) => Task.CompletedTask;

    public Task SendEmailAsync(string subject, string message) => Task.CompletedTask;
}