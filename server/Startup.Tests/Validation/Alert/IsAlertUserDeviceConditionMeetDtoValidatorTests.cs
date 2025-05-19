using Application.Models.Dtos.RestDtos;
using Application.Validation.Alert;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Startup.Tests.Validation.Alert;

[TestFixture]
public class IsAlertUserDeviceConditionMeetDtoValidatorTests
{
    private IsAlertUserDeviceConditionMeetDtoValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new IsAlertUserDeviceConditionMeetDtoValidator();
    }

    [TestCase("")]
    [TestCase(" ")]
    public void UserDeviceId_Should_Fail_When_Null_Or_Empty(string id)
    {
        var dto = CreateDto(userDeviceId: id);
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.UserDeviceId);
    }

    [Test]
    public void UserDeviceId_Should_Fail_When_Not_Guid()
    {
        var dto = CreateDto(userDeviceId: "not-a-guid");
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.UserDeviceId);
    }

    [Test]
    public void UserDeviceId_Should_Fail_When_Too_Long()
    {
        var longId = new string('a', 1001);
        var dto = CreateDto(userDeviceId: longId);
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.UserDeviceId);
    }

    [TestCase(-101)]
    [TestCase(151)]
    public void Temperature_Should_Fail_When_Out_Of_Range(double temp)
    {
        var dto = CreateDto(temperature: temp);
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Temperature);
    }

    [TestCase(-1)]
    [TestCase(101)]
    public void Humidity_Should_Fail_When_Out_Of_Range(double humidity)
    {
        var dto = CreateDto(humidity: humidity);
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Humidity);
    }

    [TestCase(0)]
    [TestCase(-5)]
    public void AirPressure_Should_Fail_When_Less_Than_Or_Equal_Zero(double pressure)
    {
        var dto = CreateDto(airPressure: pressure);
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.AirPressure);
    }

    [TestCase(-1)]
    [TestCase(2001)]
    public void AirQuality_Should_Fail_When_Out_Of_Range(int quality)
    {
        var dto = CreateDto(airQuality: quality);
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.AirQuality);
    }

    [Test]
    public void Time_Should_Fail_When_In_The_Future()
    {
        var dto = CreateDto(time: DateTime.UtcNow.AddMinutes(5));
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Time);
    }

    [Test]
    public void Valid_Dto_Should_Pass()
    {
        var dto = CreateDto(); // All defaults are valid
        _validator.TestValidate(dto).ShouldNotHaveAnyValidationErrors();
    }

    private static IsAlertUserDeviceConditionMeetDto CreateDto(
        string? userDeviceId = null,
        double? temperature = null,
        double? humidity = null,
        double? airPressure = null,
        int? airQuality = null,
        DateTime? time = null)
    {
        return new IsAlertUserDeviceConditionMeetDto
        {
            UserDeviceId = userDeviceId ?? Guid.NewGuid().ToString(),
            Temperature = temperature,
            Humidity = humidity,
            AirPressure = airPressure,
            AirQuality = airQuality,
            Time = time ?? DateTime.UtcNow
        };
    }
}