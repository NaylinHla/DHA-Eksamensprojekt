using Application.Models.Dtos.RestDtos;
using Application.Models.Dtos.RestDtos.EmailList.Request;
using Application.Validation.Email;
using Application.Validation.MQTT;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Startup.Tests.Validation.MQTT;

[TestFixture]
public class AdminChangesPreferencesDtoValidatorTests
{
    private AdminChangesPreferencesDtoValidator _adminChangesPreferencesDtoValidator;
    
    [SetUp]
    public void Init() => _adminChangesPreferencesDtoValidator = new AdminChangesPreferencesDtoValidator();

    [Test]
    public void Invalid_DeviceId_fails()
    {
        var dto = Valid();
        dto.DeviceId = "";
        _adminChangesPreferencesDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.DeviceId);
    }

    [TestCase("")]
    [TestCase("not-an-interval")]
    [TestCase("-1")]
    public void Invalid_Interval_fails(string interval)
    {
        var dto = Valid();
        dto.Interval = interval;
        _adminChangesPreferencesDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Interval);   
    }
    
    [Test]
    public void Valid_model_passes()
    {
        var dto = Valid();
        _adminChangesPreferencesDtoValidator.TestValidate(dto).ShouldNotHaveAnyValidationErrors();
    }
    
    private static AdminChangesPreferencesDto Valid() => new()
    {
        DeviceId = "valid-device-id",
        Interval = "60"
    };
}