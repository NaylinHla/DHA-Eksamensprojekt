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

    [TestCase("<=10")]
    [TestCase(">=25.5")]
    public void Condition_Should_Be_Valid(string condition)
    {
        var dto = Valid(condition: condition);
        _validator.TestValidate(dto)
            .ShouldNotHaveValidationErrorFor(x => x.Condition);
    }

    [Test]
    public void Valid_Dto_Should_Pass()
    {
        var dto = Valid();
        _validator.TestValidate(dto)
            .ShouldNotHaveAnyValidationErrors();
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