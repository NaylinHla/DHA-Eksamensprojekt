using Application.Models.Dtos.RestDtos;
using Application.Validation.AlertCondition;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Startup.Tests.Validation.AlertCondition
{
    [TestFixture]
    public class ConditionAlertPlantCreateDtoValidatorTests
    {
        private ConditionAlertPlantCreateDtoValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _validator = new ConditionAlertPlantCreateDtoValidator();
        }

        [Test]
        public void PlantId_Should_Have_Error_When_Empty_Guid()
        {
            var dto = new ConditionAlertPlantCreateDto
            {
                PlantId = Guid.Empty
            };

            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.PlantId);
        }

        [Test]
        public void PlantId_Should_Not_Have_Error_When_Valid_Guid()
        {
            var dto = new ConditionAlertPlantCreateDto
            {
                PlantId = Guid.NewGuid()
            };

            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.PlantId);
        }
    }
}