using Application.Interfaces.Infrastructure.Postgres;
using Application.Models;
using Application.Services;
using Core.Domain.Entities;
using Moq;
using NUnit.Framework;

namespace Startup.Tests.UserSettingsTests
{
    [TestFixture]
    public class UserSettingsServiceTests
    {
        private Mock<IUserSettingsRepository> _repo;
        private UserSettingsService   _service;
        private readonly Guid         _userId = Guid.NewGuid();
        private JwtClaims             _claims;
        private UserSettings          _existingSettings;

        [SetUp]
        public void SetUp()
        {
            _repo = new Mock<IUserSettingsRepository>(MockBehavior.Strict);
            _service = new UserSettingsService(_repo.Object);
            _claims = new JwtClaims
            {
                Id = _userId.ToString(),
                Country = "Denmark",
                Email = "This@IsAnEmail.com",
                Exp = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Role = "user"
            };

            _existingSettings = new UserSettings
            {
                UserId        = _userId,
                Celsius       = false,
                DarkTheme     = false,
                ConfirmDialog = false,
                SecretMode    = false
            };
        }

        [Test]
        public void UpdateSetting_SettingsNotFound_ThrowsKeyNotFoundException()
        {
            _repo
                .Setup(r => r.GetByUserId(_userId))
                .Returns((UserSettings?)null);

            var ex = Assert.Throws<KeyNotFoundException>(
                () => _service.UpdateSetting("celsius", true, _claims)
            );
            Assert.That(ex.Message, Is.EqualTo("User settings not found"));
        }

        [Test]
        public void GetSettings_SettingsNotFound_ThrowsKeyNotFoundException()
        {
            _repo
                .Setup(r => r.GetByUserId(_userId))
                .Returns((UserSettings?)null);

            var ex = Assert.Throws<KeyNotFoundException>(
                () => _service.GetSettings(_claims)
            );
            Assert.That(ex.Message, Is.EqualTo("User settings not found"));
        }

        [TestCase("celsius")]
        [TestCase("Celsius")]
        public void UpdateSetting_ValidSettingName_CallsRepositoryUpdate(string settingName)
        {
            _repo
                .Setup(r => r.GetByUserId(_userId))
                .Returns(_existingSettings);
            _repo
                .Setup(r => r.Update(It.Is<UserSettings>(s => s.Celsius == true)));

            _service.UpdateSetting(settingName, true, _claims);

            _repo.Verify(r => r.Update(It.IsAny<UserSettings>()), Times.Once);
        }

        [Test]
        public void GetSettings_ExistingSettings_ReturnsThem()
        {
            _repo
                .Setup(r => r.GetByUserId(_userId))
                .Returns(_existingSettings);

            var result = _service.GetSettings(_claims);

            Assert.That(result, Is.SameAs(_existingSettings));
        }
    }
}
