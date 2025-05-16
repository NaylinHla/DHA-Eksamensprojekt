using Application.Interfaces.Infrastructure.Postgres;
using Application.Models.Dtos.RestDtos;
using Application.Validation.Auth;
using FluentValidation.TestHelper;
using Moq;
using NUnit.Framework;

namespace Startup.Tests.Validation.Auth;

[TestFixture]
public class AuthRegisterDtoValidatorTests
{
    private AuthRegisterDtoValidator _authRegisterDtoValidator;

    [SetUp]
    public void SetUp()
    {
        var repo = Mock.Of<IUserRepository>(r =>
            r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()) == Task.FromResult(false));
        _authRegisterDtoValidator = new AuthRegisterDtoValidator(repo);
    }

    [TestCase("")]
    [TestCase("1")]
    [TestCase("FirstNamesCannotBeLongerThan50CharactersAndThisIsLongerThanThat")]
    public async Task Invalid_FirstName_Fails(string firstName)
    {
        var dto = Valid();
        dto.FirstName = firstName;
        var result = await _authRegisterDtoValidator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [TestCase("")]
    [TestCase("1")]
    [TestCase("LastNamesCannotBeLongerThan50CharactersAndThisIsLongerThanThat")]
    public async Task Invalid_LastName_Fails(string lastName)
    {
        var dto = Valid();
        dto.LastName = lastName;
        var result = await _authRegisterDtoValidator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Test]
    public async Task Invalid_Birthday_Fails()
    {
        var dto = Valid();
        dto.Birthday = DateTime.UtcNow;
        var result = await _authRegisterDtoValidator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Birthday);
    }

    [TestCase("")]
    [TestCase("Not-valid-email")]
    public async Task Invalid_email_fails(string email)
    {
        var dto = Valid();
        dto.Email = email;
        var result = await _authRegisterDtoValidator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
    
    [TestCase("")]
    [TestCase("123")]
    public async Task Invalid_Password_Fails(string pass)
    {
        var dto = Valid();
        dto.Password = pass;
        var result = await _authRegisterDtoValidator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
    
    [TestCase("")]
    [TestCase("ThisIsAnExampleThatYouCannotInputACountryWithMoreThan56Characters")]
    public async Task Invalid_Country_Fails(string country)
    {
        var dto = Valid();
        dto.Country = country;
        var result = await _authRegisterDtoValidator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(x => x.Country);
    }
    
    [Test]
    public async Task Valid_model_passes()
    {
        var dto = Valid();
        var result = await _authRegisterDtoValidator.TestValidateAsync(dto);
        result.ShouldNotHaveAnyValidationErrors();
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