using Application.Models.Dtos.RestDtos.UserDevice.Request;
using Application.Validation.UserDevice;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Startup.Tests.Validation.UserDevice;

[TestFixture]
public class UserDeviceCreateDtoValidatorTests
{
    private UserDeviceCreateDtoValidator _userDeviceCreateDtoValidator;

    [SetUp]
    public void Init() => _userDeviceCreateDtoValidator = new UserDeviceCreateDtoValidator();

    [TestCase("")]
    [TestCase("1")]
    [TestCase("DeviceNamesCannotBeLongerThan50CharactersAndThisIsLongerThanThat")]
    public void Invalid_DeviceName_Fails(string deviceName)
    {
        var dto = Valid();
        dto.DeviceName = deviceName;
        _userDeviceCreateDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.DeviceName);
    }

    [Test]
    public void DeviceDescription_Cannot_Be_More_Than_500_characters()
    {
        var dto = Valid();
        dto.DeviceDescription = new string('a', 501);
        _userDeviceCreateDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.DeviceDescription);
    }
        
    [TestCase("a")]
    [TestCase("")]
    [TestCase("-1")]
    public void WaitTime_must_be_digits(string waitTime)
    {
        var dto = Valid();
        dto.WaitTime = waitTime;;
        _userDeviceCreateDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.WaitTime);
    }

    [Test]
    public void Created_Cannot_Be_In_Future()
    {
        var dto = Valid();
        dto.Created = DateTime.UtcNow.AddDays(1);
        _userDeviceCreateDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Created);   
    }
    
    [Test]
    public void Valid_model_passes()
    {
        var dto = Valid();
        _userDeviceCreateDtoValidator.TestValidate(dto).ShouldNotHaveAnyValidationErrors();
    }

    private static UserDeviceCreateDto Valid() => new()
    {
        DeviceName  = "TestDevice",
        DeviceDescription  = "This Is a test description",
        Created = DateTime.UtcNow,
        WaitTime = "60"
    };
}