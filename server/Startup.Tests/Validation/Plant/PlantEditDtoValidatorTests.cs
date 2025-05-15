using Application.Models.Dtos.RestDtos;
using Application.Validation.Plant;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Startup.Tests.Validation.Plant;

[TestFixture]
public class PlantEditDtoValidatorTests
{
    private PlantEditDtoValidator _plantEditDtoValidator;

    [SetUp]
    public void Init() => _plantEditDtoValidator = new PlantEditDtoValidator();

    [TestCase("")]
    [TestCase("ThisIsAnExampleOfATooLongNameThisIsAnExampleOfATooLongNameThisIsAnExampleOfATooLongNameThisIsAnExampleOfATooLongName")]
    public void Invalid_PlantName_fails(string name)
    {
        var dto = Valid();
        dto.PlantName = name;
        _plantEditDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.PlantName);
    }
    
    [TestCase("")]
    [TestCase("ThisIsAnExampleOfATooLongPlantTypeThisIsAnExampleOfATooLongPlantType")]
    public void Invalid_PlantType_fails(string plantType)
    {
        var dto = Valid();
        dto.PlantType = plantType;
        _plantEditDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.PlantType);
    }
    
    [TestCase(0)]
    [TestCase(366)]
    public void Invalid_WaterEvery_fails(int waterEvery)
    {
        var dto = Valid();
        dto.WaterEvery = waterEvery;
        _plantEditDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.WaterEvery);
    }

    [Test]
    public void PlantNotes_Cannot_Be_More_Than_1000_Characters()
    {
        var dto = Valid();
        dto.PlantNotes = new string('a', 1001);
        _plantEditDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.PlantNotes);
    }

    [Test]
    public void LastWatered_Cannot_Be_In_Future()
    {
        var dto = Valid();
        dto.LastWatered = DateTime.UtcNow.AddDays(1);
        _plantEditDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.LastWatered);
    }
    
    [Test]
    public void Valid_model_passes()
    {
        var dto = Valid();
        _plantEditDtoValidator.TestValidate(dto).ShouldNotHaveAnyValidationErrors();
    }

    private static PlantEditDto Valid() => new()
    {
        PlantName  = "Test Plant",
        PlantType  = "Rose",
        PlantNotes = "This is not Null",
        LastWatered = DateTime.UtcNow,
        WaterEvery = 3
    };
}