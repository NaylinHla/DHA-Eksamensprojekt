using Application.Models.Dtos.RestDtos;
using Application.Validation.Auth;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Startup.Tests.Validation.Auth;

[TestFixture]
public class AuthRegisterDtoValidatorTests
{
    private AuthRegisterDtoValidator _authRegisterDtoValidator;
    
    [SetUp]
    public void Init() => _authRegisterDtoValidator = new AuthRegisterDtoValidator();

    [TestCase("")]
    [TestCase("1")]
    [TestCase("FirstNamesCannotBeLongerThan50CharactersAndThisIsLongerThanThat")]
    public void Invalid_FirstName_Fails(string firstName)
    {
        var dto = Valid();
        dto.FirstName = firstName;
        _authRegisterDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [TestCase("")]
    [TestCase("1")]
    [TestCase("LastNamesCannotBeLongerThan50CharactersAndThisIsLongerThanThat")]
    public void Invalid_LastName_Fails(string lastName)
    {
        var dto = Valid();
        dto.LastName = lastName;
        _authRegisterDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Test]
    public void Invalid_Birthday_Fails()
    {
        var dto = Valid();
        dto.Birthday = DateTime.UtcNow.AddYears(-1);
        _authRegisterDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Birthday);
    }

    [TestCase("")]
    [TestCase("Not-valid-email")]
    public void Invalid_email_fails(string email)
    {
        var dto = Valid();
        dto.Email = email;
        _authRegisterDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Email);
    }
    
    [TestCase("")]
    [TestCase("123")]
    public void Invalid_Password_Fails(string pass)
    {
        var dto = Valid();
        dto.Password = pass;
        _authRegisterDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Password);
    }
    
    [TestCase("")]
    [TestCase("ThisIsAnExampleThatYouCannotInputACountryWithMoreThan56Characters")]
    public void Invalid_Country_Fails(string country)
    {
        var dto = Valid();
        dto.Country = country;
        _authRegisterDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Country);
    }
    
    [Test]
    public void Valid_model_passes()
    {
        var dto = Valid();
        _authRegisterDtoValidator.TestValidate(dto).ShouldNotHaveAnyValidationErrors();
    }
    
    private static AuthRegisterDto Valid() => new()
    {
        FirstName = "ValidFirstName",
        LastName = "ValidLastName",
        Email = "valid@email.com",
        Birthday = DateTime.UtcNow.AddYears(-18),
        Country = "ValidCountry",
        Password = "ValidPassword"
    };
}