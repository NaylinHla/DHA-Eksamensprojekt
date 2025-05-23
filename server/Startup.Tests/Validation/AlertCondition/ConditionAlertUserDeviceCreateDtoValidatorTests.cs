using Application.Models.Dtos.RestDtos;
using Application.Validation.AlertCondition;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Startup.Tests.Validation.AlertCondition;

[TestFixture]
public class ConditionAlertUserDeviceCreateDtoValidatorTests
{
    private ConditionAlertUserDeviceCreateDtoValidator _validator;

    [SetUp]
    public void SetUp() =>
        _validator = new ConditionAlertUserDeviceCreateDtoValidator();

    // --- UserDeviceId Tests ---

    [TestCase("")]
    public void UserDeviceId_Should_Have_Error_When_Empty(string userDeviceId)
    {
        var dto = Valid(userDeviceId: userDeviceId);
        _validator.TestValidate(dto)
            .ShouldHaveValidationErrorFor(x => x.UserDeviceId);
    }

    [Test]
    public void UserDeviceId_Should_Have_Error_When_Not_Valid_Guid()
    {
        var dto = Valid(userDeviceId: "not-a-guid");
        _validator.TestValidate(dto)
            .ShouldHaveValidationErrorFor(x => x.UserDeviceId);
    }

    [Test]
    public void UserDeviceId_Should_Have_Error_When_Too_Long()
    {
        var dto = Valid(userDeviceId: new string('a', 1001));
        _validator.TestValidate(dto)
            .ShouldHaveValidationErrorFor(x => x.UserDeviceId);
    }

    [Test]
    public void UserDeviceId_Should_Be_Valid()
    {
        var dto = Valid(userDeviceId: Guid.NewGuid().ToString());
        _validator.TestValidate(dto)
            .ShouldNotHaveValidationErrorFor(x => x.UserDeviceId);
    }

    // --- SensorType Tests ---

    [TestCase("")]
    [TestCase("CO2")]
    [TestCase("temperature")] // case-sensitive
    public void SensorType_Should_Have_Error_When_Invalid(string sensorType)
    {
        var dto = Valid(sensorType: sensorType);
        _validator.TestValidate(dto)
            .ShouldHaveValidationErrorFor(x => x.SensorType);
    }

    [Test]
    public void SensorType_Should_Have_Error_When_Too_Long()
    {
        var dto = Valid(sensorType: new string('X', 51));
        _validator.TestValidate(dto)
            .ShouldHaveValidationErrorFor(x => x.SensorType);
    }

    [TestCase("Temperature")]
    [TestCase("Humidity")]
    [TestCase("AirPressure")]
    [TestCase("AirQuality")]
    public void SensorType_Should_Be_Valid(string sensorType)
    {
        var dto = Valid(sensorType: sensorType);
        _validator.TestValidate(dto)
            .ShouldNotHaveValidationErrorFor(x => x.SensorType);
    }

    // --- Condition Tests ---

    [TestCase("")]
    [TestCase("==20")]
    [TestCase(">20")]
    [TestCase("<=abc")]
    public void Condition_Should_Have_Error_When_Invalid_Format(string condition)
    {
        var dto = Valid(condition: condition);
        _validator.TestValidate(dto)
            .ShouldHaveValidationErrorFor(x => x.Condition);
    }

    [Test]
    public void Condition_Should_Have_Error_When_Too_Long()
    {
        var dto = Valid(condition: new string('>', 21));
        _validator.TestValidate(dto)
            .ShouldHaveValidationErrorFor(x => x.Condition);
    }

    [Test]
    public void Valid_Dto_Should_Pass()
    {
        var dto = Valid();
        _validator.TestValidate(dto)
            .ShouldNotHaveAnyValidationErrors();
    }
    
    [TestCase("Temperature", "<=-41")]
    [TestCase("Temperature", ">=131")]
    [TestCase("Temperature", "<=-40.01")]
    [TestCase("Temperature", ">=130.01")]
    [TestCase("Humidity", "<=-1")]
    [TestCase("Humidity", ">=101")]
    [TestCase("Humidity", "<=-0.01")]
    [TestCase("Humidity", ">=100.01")]
    [TestCase("AirPressure", "<=0")]
    [TestCase("AirPressure", "<=-0.01")]
    [TestCase("AirQuality", "<=-1")]
    [TestCase("AirQuality", ">=2001")]
    [TestCase("AirQuality", "<=-0.01")]
    [TestCase("AirQuality", ">=2000.01")]
    public void Condition_Should_Have_Error_When_Value_Just_Outside_Range(string sensorType, string condition)
    {
        var dto = Valid(sensorType: sensorType, condition: condition);
        _validator.TestValidate(dto)
            .ShouldHaveValidationErrorFor(x => x.Condition);
    }

    [TestCase("Temperature", "<=-40")]
    [TestCase("Temperature", ">=130")]
    [TestCase("Humidity", "<=0")]
    [TestCase("Humidity", ">=100")]
    [TestCase("AirPressure", ">=0.01")]
    [TestCase("AirQuality", "<=0")]
    [TestCase("AirQuality", ">=2000")]
    public void Condition_Should_Be_Valid_At_Range_Edges(string sensorType, string condition)
    {
        var dto = Valid(sensorType: sensorType, condition: condition);
        _validator.TestValidate(dto)
            .ShouldNotHaveValidationErrorFor(x => x.Condition);
    }

    // --- Helper Factory ---

    private static ConditionAlertUserDeviceCreateDto Valid(
        string? userDeviceId = null,
        string? sensorType = null,
        string? condition = null)
    {
        return new ConditionAlertUserDeviceCreateDto
        {
            UserDeviceId = userDeviceId ?? Guid.NewGuid().ToString(),
            SensorType = sensorType ?? "Temperature",
            Condition = condition ?? ">=20"
        };
    }
}