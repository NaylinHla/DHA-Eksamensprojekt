using Application.Models.Dtos.MqttSubscriptionDto;
using Application.Validation.MQTT;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Startup.Tests.Validation.MQTT;

[TestFixture]
public class DeviceSensorDataDtoValidatorTests
{
    private DeviceSensorDataDtoValidator _deviceSensorDataDtoValidator;

    [SetUp]
    public void Init() => _deviceSensorDataDtoValidator = new DeviceSensorDataDtoValidator();

    [TestCase("")]
    [TestCase("not-guid")]
    public void Invalid_DeviceId_fails(string deviceId)
    {
        var dto = Valid();
        dto.DeviceId = deviceId;
        _deviceSensorDataDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.DeviceId);
    }

    [TestCase(-41)]
    [TestCase(131)]
    public void Temperature_out_of_range_fails(int temp)
    {
        var dto = Valid();
        dto.Temperature = temp;
        _deviceSensorDataDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Temperature);
    }
    
    [TestCase(-1)]
    [TestCase(101)]
    public void Humidity_out_of_range_fails(int humidity)
    {
        var dto = Valid();
        dto.Humidity = humidity;
        _deviceSensorDataDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Humidity);
    }

    [Test]
    public void AirPressure_out_of_range_fails()
    {
        var dto = Valid();
        dto.AirPressure = -1;
        _deviceSensorDataDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.AirPressure);
    }
    
    [TestCase(-1)]
    [TestCase(2001)]
    public void AirQuality_out_of_range_fails(int airQuality)
    {
        var dto = Valid();
        dto.AirQuality = airQuality;
        _deviceSensorDataDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.AirQuality);
    }

    [Test]
    public void Time_Cannot_be_In_Future()
    {
        var dto = Valid();
        dto.Time = DateTime.UtcNow.AddDays(1);
        _deviceSensorDataDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Time);
    }

    [Test]
    public void Happy_path()
    {
        var dto = Valid();
        _deviceSensorDataDtoValidator.TestValidate(dto).ShouldNotHaveAnyValidationErrors();
    }

    private static DeviceSensorDataDto Valid() => new()
    {
        DeviceId    = Guid.NewGuid().ToString(),
        Temperature = 23,
        Humidity    = 55,
        AirPressure = 1013,
        Time        = DateTime.UtcNow
    };
}