using Application.Models.Dtos.RestDtos;
using Application.Validation.Plant;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Startup.Tests.Validation.Plant;

[TestFixture]
public class PlantCreateDtoValidatorTests
{
    private PlantCreateDtoValidator _plantCreateDtoValidator;

    [SetUp]
    public void Init() => _plantCreateDtoValidator = new PlantCreateDtoValidator();

    [TestCase("")]
    [TestCase(
        "ThisIsAnExampleOfATooLongNameThisIsAnExampleOfATooLongNameThisIsAnExampleOfATooLongNameThisIsAnExampleOfATooLongName")]
    public void Invalid_PlantName_fails(string name)
    {
        var dto = Valid();
        dto.PlantName = name;
        _plantCreateDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.PlantName);
    }
    
    [TestCase("")]
    [TestCase("ThisIsAnExampleOfATooLongPlantTypeThisIsAnExampleOfATooLongPlantType")]
    public void Invalid_PlantType_fails(string plantType)
    {
        var dto = Valid();
        dto.PlantType = plantType;
        _plantCreateDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.PlantType);
    }
    
    [TestCase(0)]
    [TestCase(366)]
    public void Invalid_WaterEvery_fails(int waterEvery)
    {
        var dto = Valid();
        dto.WaterEvery = waterEvery;
        _plantCreateDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.WaterEvery);
    }

    [Test]
    public void Planted_Cannot_Be_In_Future()
    {
        var dto = Valid();
        dto.Planted = DateTime.UtcNow.AddDays(1);
        _plantCreateDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Planted);
    }

    [Test]
    public void PlantNotes_Cannot_Be_More_Than_1000_Characters()
    {
        var dto = Valid();
        dto.PlantNotes = new string('a', 1001);
        _plantCreateDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.PlantNotes);
    }

    [Test]
    public void IsDead_Cannot_Be_True_When_Creating_A_Plant()
    {
        var dto = Valid();
        dto.IsDead = true;
        _plantCreateDtoValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.IsDead);
    }
    
    [Test]
    public void Valid_model_passes()
    {
        var dto = Valid();
        _plantCreateDtoValidator.TestValidate(dto).ShouldNotHaveAnyValidationErrors();
    }

    private static PlantCreateDto Valid() => new()
    {
        PlantName  = "Test Plant",
        PlantType  = "Rose",
        Planted = DateTime.UtcNow,
        PlantNotes = "This is not Null",
        WaterEvery = 3,
        IsDead = false
    };
}