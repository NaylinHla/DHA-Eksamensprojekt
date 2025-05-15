using Application.Models.Dtos.RestDtos.EmailList.Request;
using Application.Validation.Email;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Startup.Tests.Validation.Email;

[TestFixture]
public class AddEmailDtoValidatorTests
{
    private AddEmailDtoValidator _addEmailDtoValidator;
    
    [SetUp]
    public void Init() => _addEmailDtoValidator = new AddEmailDtoValidator();
    
    [TestCase("")]
    [TestCase("not-an-email")]
    public void Invalid_email_fails(string email)
    {
        var dto = Valid();
        dto.Email = email;
        _addEmailDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Email);
    }
    
    [Test]
    public void Valid_model_passes()
    {
        var dto = Valid();
        _addEmailDtoValidator.TestValidate(dto).ShouldNotHaveAnyValidationErrors();
    }
    
    private static AddEmailDto Valid() => new()
    {
        Email = "valid@email.com"
    };
}