using Application.Interfaces.Infrastructure.Postgres;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Services;
using Moq;
using NUnit.Framework;

namespace Startup.Tests.GreenhouseDeviceTests
{
    public class GreenhouseDeviceServiceTests
    {
        private GreenhouseDeviceService _service = null!;
        private Mock<IGreenhouseDeviceRepository> _repositoryMock = null!;
        private Mock<IConnectionManager> _connectionManagerMock = null!;
        private Mock<IServiceProvider> _serviceProviderMock = null!;

        [SetUp]
        public void Setup()
        {
            // Mock the repository and connection manager
            _repositoryMock = new Mock<IGreenhouseDeviceRepository>();
            _connectionManagerMock = new Mock<IConnectionManager>();

            // Mock IServiceProvider to resolve the repository
            _serviceProviderMock = new Mock<IServiceProvider>();
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(IGreenhouseDeviceRepository)))
                .Returns(_repositoryMock.Object);

            // Create GreenhouseDeviceService with mocked dependencies
            _service = new GreenhouseDeviceService(_serviceProviderMock.Object, _connectionManagerMock.Object);
        }

        [Test]
        public void AddToDbAndBroadcast_ShouldReturnEarly_WhenDtoIsNull()
        {
            // Act & Assert: Ensure no exception is thrown when dto is null
            Assert.DoesNotThrowAsync(async () => await _service.AddToDbAndBroadcast(null));
        }
    }
}