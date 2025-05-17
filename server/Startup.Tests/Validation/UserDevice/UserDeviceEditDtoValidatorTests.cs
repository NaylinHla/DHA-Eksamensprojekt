using Application.Models.Dtos.RestDtos.UserDevice.Request;
using Application.Validation.UserDevice;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Startup.Tests.Validation.UserDevice;

[TestFixture]
public class UserDeviceEditDtoValidatorTests
{
    private UserDeviceEditDtoValidator _userDeviceEditDtoValidator;

    [SetUp]
    public void Init() => _userDeviceEditDtoValidator = new UserDeviceEditDtoValidator();

    [TestCase("")]
    [TestCase("1")]
    [TestCase("DeviceNamesCannotBeLongerThan50CharactersAndThisIsLongerThanThat")]
    public void Invalid_DeviceName_Fails(string deviceName)
    {
        var dto = Valid();
        dto.DeviceName = deviceName;
        _userDeviceEditDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.DeviceName);
    }

    [Test]
    public void DeviceDescription_Cannot_Be_More_Than_500_characters()
    {
        var dto = Valid();
        dto.DeviceDescription = new string('a', 501);
        _userDeviceEditDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.DeviceDescription);
    }
    
    [TestCase("a")]
    [TestCase("")]
    [TestCase("-1")]
    public void Invalid_WaitTime_Fails(string waitTime)
    {
        var dto = Valid();
        dto.WaitTime = waitTime;
        _userDeviceEditDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.WaitTime);
    }
    
    [Test]
    public void Valid_model_passes()
    {
        var dto = Valid();
        _userDeviceEditDtoValidator.TestValidate(dto).ShouldNotHaveAnyValidationErrors();
    }

    private static UserDeviceEditDto Valid() => new()
    {
        DeviceName  = "TestDevice",
        DeviceDescription  = "This Is a test description",
        WaitTime = "60"
    };
}