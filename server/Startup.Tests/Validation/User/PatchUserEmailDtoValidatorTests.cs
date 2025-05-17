using Application.Interfaces.Infrastructure.Postgres;
using Application.Models.Dtos.RestDtos.Request;
using Application.Validation.User;
using FluentValidation.TestHelper;
using Moq;
using NUnit.Framework;

namespace Startup.Tests.Validation.User;

[TestFixture]
public class PatchUserEmailDtoValidatorTests
{
    private PatchUserEmailDtoValidator _patchUserEmailDtoValidator;

    [SetUp]
    public void SetUp()
    {
        var repo = Mock.Of<IUserRepository>(r =>
            r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()) == Task.FromResult(false));
        _patchUserEmailDtoValidator = new PatchUserEmailDtoValidator(repo);
    }

    [TestCase("")]
    [TestCase("not-an-email")]
    public async Task Invalid_old_email_fails(string email)
    {
        var dto = Valid();
        dto.OldEmail = email;
        var result = await _patchUserEmailDtoValidator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.OldEmail);
    }
    
    [TestCase("")]
    [TestCase("not-an-email")]
    public async Task Invalid_new_email_fails(string email)
    {
        var dto = Valid();
        dto.NewEmail = email;
        var result = await _patchUserEmailDtoValidator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.NewEmail);
    }
    
    [Test]
    public async Task Valid_model_passes()
    {
        var dto = Valid();
        var result = await _patchUserEmailDtoValidator.TestValidateAsync(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    private static PatchUserEmailDto Valid() => new()
    {
        OldEmail = "Valid@Email.com",
        NewEmail = "AlsoValid@Gmail.com"
    };
}