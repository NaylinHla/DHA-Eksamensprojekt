using Application.Models.Dtos.RestDtos;
using Application.Validation.Alert;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Startup.Tests.Validation.Alert;

[TestFixture]
public class AlertCreateDtoValidatorTests
{
    private AlertCreateDtoValidator _alertCreateDtoValidator;

    [SetUp]
    public void SetUp() => _alertCreateDtoValidator = new AlertCreateDtoValidator();

    [TestCase("")]
    [TestCase(
        "thisIsForATestToSeeIfAnAlertNameCanBeMoreThanAHundredCharactersThisIsForATestToSeeIfAnAlertNameCanBeMoreThanAHundredCharacters")]
    public void Invalid_AlertName_fails(string name)
    {
        var dto = Valid();
        dto.AlertName = name;
        _alertCreateDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.AlertName);
    }

    [TestCase("")]
    [TestCase("1234")]
    public void Invalid_AlertDesc_fails(string desc)
    {
        var dto = Valid();
        dto.AlertDesc = desc;
        _alertCreateDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.AlertDesc);
    }
    
    [Test]
    public void AlertConditionId_Cannot_Be_Null()
    {
        var dto = Valid();
        dto.AlertConditionId = null;
        _alertCreateDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.AlertConditionId);
    }

    [Test]
    public void AlertUser_Cannot_Be_Null()
    {
        var dto = Valid();
        dto.AlertUser = null;
        _alertCreateDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.AlertUser);
    }
    
    [Test]
    public void Valid_model_passes()
    {
        var dto = Valid();
        _alertCreateDtoValidator.TestValidate(dto).ShouldNotHaveAnyValidationErrors();
    }
    
    private static AlertCreateDto Valid() => new()
    {
        AlertName  = "Test Alert",
        AlertDesc  = "This Is a test description",
        AlertConditionId = Guid.NewGuid(),
        IsPlantCondition = true,
        AlertUser = Guid.NewGuid()
    };
}