using Application.Models.Dtos.RestDtos.Request;
using Application.Validation.User;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Startup.Tests.Validation.User;

[TestFixture]
public class DeleteUserDtoValidatorTests
{
    private DeleteUserDtoValidator _deleteUserDtoValidator;

    [SetUp]
    public void Init() => _deleteUserDtoValidator = new DeleteUserDtoValidator();

    [TestCase("")]
    [TestCase("not-an-email")]
    public void Invalid_email_fails(string email)
    {
        var dto = Valid();
        dto.Email = email;
        _deleteUserDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Email);
    }
    
    [Test]
    public void Valid_model_passes()
    {
        var dto = Valid();
        _deleteUserDtoValidator.TestValidate(dto).ShouldNotHaveAnyValidationErrors();
    }

    private static DeleteUserDto Valid() => new()
    {
        Email = "Valid@Email.com"
    };
}