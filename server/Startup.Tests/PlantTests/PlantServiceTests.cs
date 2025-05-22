
using Application.Interfaces.Infrastructure.Postgres;
using Application.Models;
using Core.Domain.Entities;
using FluentValidation;
using Moq;
using NUnit.Framework;
using Application.Models.Dtos.RestDtos;
using Application.Services;

namespace Startup.Tests.PlantTests
{
    [TestFixture]
    public class PlantServiceTests
    {
        private Mock<IPlantRepository> _repo;
        private Mock<IValidator<PlantCreateDto>> _createValidator;
        private Mock<IValidator<PlantEditDto>> _editValidator;
        private PlantService _service;
        private JwtClaims _claims;

        [SetUp]
        public void Setup()
        {
            _repo = new Mock<IPlantRepository>(MockBehavior.Strict);
            _createValidator = new Mock<IValidator<PlantCreateDto>>(MockBehavior.Strict);
            _editValidator   = new Mock<IValidator<PlantEditDto>>(MockBehavior.Strict);

            _service = new PlantService(
                _repo.Object,
                _createValidator.Object,
                _editValidator.Object
            );
            
            _claims = new JwtClaims
            {
                Id = Guid.NewGuid().ToString(),
                Role = "user",
                Email = "<EMAIL>",
                Exp = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Country = "Denmark"
            };
        }

        // helper to stub null plant
        private void StubPlantNotFound(Guid plantId)
        {
            _repo
                .Setup(r => r.GetPlantByIdAsync(plantId))
                .ReturnsAsync((Plant?)null);
        }

        [TestCase(nameof(PlantService.EditPlantAsync))]
        [TestCase(nameof(PlantService.MarkPlantAsDeadAsync))]
        [TestCase(nameof(PlantService.WaterPlantAsync))]
        public void ServiceMethods_WhenPlantNotFound_ThrowKeyNotFoundException(string methodName)
        {
            // Arrange
            var plantId = Guid.NewGuid();
            StubPlantNotFound(plantId);

            // Act & Assert
            AsyncTestDelegate action = methodName switch
            {
                nameof(PlantService.EditPlantAsync)        => () => _service.EditPlantAsync(plantId, new PlantEditDto(), _claims),
                nameof(PlantService.MarkPlantAsDeadAsync)  => () => _service.MarkPlantAsDeadAsync(plantId, _claims),
                nameof(PlantService.WaterPlantAsync)       => () => _service.WaterPlantAsync(plantId, _claims),
                _ => throw new ArgumentException($"Unknown method {methodName}")
            };

            Assert.ThrowsAsync<KeyNotFoundException>(action);
        }
    }
}
