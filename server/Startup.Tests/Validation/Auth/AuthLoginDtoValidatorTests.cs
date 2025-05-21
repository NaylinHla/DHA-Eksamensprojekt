using Application.Models.Dtos.RestDtos;
using Application.Validation.Auth;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Startup.Tests.Validation.Auth;

[TestFixture]
public class AuthLoginDtoValidatorTests
{
    private AuthLoginDtoValidator _authLoginDtoValidator;

    [SetUp]
    public void Init() => _authLoginDtoValidator = new AuthLoginDtoValidator();

    [TestCase("not-an-email")]
    [TestCase("123456")]
    [TestCase("")]
    public void Invalid_email_fails(string email)
    {
        var dto = Valid();
        dto.Email = email;
        _authLoginDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Email);
    }

    [TestCase("")]
    [TestCase("123")]
    public void Empty_password_fails(string pass)
    {
        var dto = Valid();
        dto.Password = pass;
        _authLoginDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Test]
    public void Valid_model_passes()
    {
        var dto = Valid();
        _authLoginDtoValidator.TestValidate(dto).ShouldNotHaveAnyValidationErrors();
    }
    
    private static AuthLoginDto Valid() => new()
    {
        Email = "valid@email.com",
        Password = "ValidPassword!1"
    };
}