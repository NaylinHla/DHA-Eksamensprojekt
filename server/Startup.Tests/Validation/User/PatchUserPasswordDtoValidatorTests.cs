using Application.Models.Dtos.RestDtos.Request;
using Application.Validation.User;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Startup.Tests.Validation.User;

[TestFixture]
public class PatchUserPasswordDtoValidatorTests
{
    private PatchUserPasswordDtoValidator _patchUserPasswordDtoValidator;

    [SetUp]
    public void Init() => _patchUserPasswordDtoValidator = new PatchUserPasswordDtoValidator();

    [TestCase("")]
    [TestCase("123")]
    public void Invalid_Old_Password_Fails(string email)
    {
        var dto = Valid();
        dto.OldPassword = email;
        _patchUserPasswordDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.OldPassword);
    }
    
    [TestCase("")]
    [TestCase("123")]
    public void Invalid_New_Password_Fails(string email)
    {
        var dto = Valid();
        dto.NewPassword = email;
        _patchUserPasswordDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.NewPassword);
    }
    
    [Test]
    public void Valid_model_passes()
    {
        var dto = Valid();
        _patchUserPasswordDtoValidator.TestValidate(dto).ShouldNotHaveAnyValidationErrors();
    }

    private static PatchUserPasswordDto Valid() => new()
    {
        OldPassword = "ValidPassword",
        NewPassword = "NewValidPassword"
    };
}