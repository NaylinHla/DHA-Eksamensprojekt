using Application.Models.Dtos.RestDtos.Request;
using Application.Validation.User;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Startup.Tests.Validation.User;

[TestFixture]
public class PatchUserEmailDtoValidatorTests
{
    private PatchUserEmailDtoValidator _patchUserEmailDtoValidator;

    [SetUp]
    public void Init() => _patchUserEmailDtoValidator = new PatchUserEmailDtoValidator();

    [TestCase("")]
    [TestCase("not-an-email")]
    public void Invalid_old_email_fails(string email)
    {
        var dto = Valid();
        dto.OldEmail = email;
        _patchUserEmailDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.OldEmail);
    }
    
    [TestCase("")]
    [TestCase("not-an-email")]
    public void Invalid_new_email_fails(string email)
    {
        var dto = Valid();
        dto.NewEmail = email;
        _patchUserEmailDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.NewEmail);
    }
    
    [Test]
    public void Valid_model_passes()
    {
        var dto = Valid();
        _patchUserEmailDtoValidator.TestValidate(dto).ShouldNotHaveAnyValidationErrors();
    }

    private static PatchUserEmailDto Valid() => new()
    {
        OldEmail = "Valid@Email.com",
        NewEmail = "AlsoValid@Gmail.com"
    };
}