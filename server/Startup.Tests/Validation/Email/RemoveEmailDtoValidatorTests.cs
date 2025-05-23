using Application.Models.Dtos.RestDtos.EmailList.Request;
using Application.Validation.Email;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Startup.Tests.Validation.Email;

[TestFixture]
public class RemoveEmailDtoValidatorTests
{
    private RemoveEmailDtoValidator _removeEmailDtoValidator;
    
    [SetUp]
    public void Init() => _removeEmailDtoValidator = new RemoveEmailDtoValidator();
    
    [TestCase("")]
    [TestCase("not-an-email")]
    public void Invalid_email_fails(string email)
    {
        var dto = Valid();
        dto.Email = email;
        _removeEmailDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Email);
    }
    
    [Test]
    public void Valid_model_passes()
    {
        var dto = Valid();
        _removeEmailDtoValidator.TestValidate(dto).ShouldNotHaveAnyValidationErrors();
    }
    
    private static RemoveEmailDto Valid() => new()
    {
        Email = "valid@email.com"
    };
}