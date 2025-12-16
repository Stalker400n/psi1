using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using back.Controllers;
using back.Models;
using back.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace back.Tests.Controllers
{
    public class GlobalUsersControllerTests
    {
        private readonly Mock<IGlobalUsersRepository> _globalUsersRepositoryMock;
        private readonly Mock<ILogger<GlobalUsersController>> _loggerMock;
        private readonly GlobalUsersController _sut;

        public GlobalUsersControllerTests()
        {
            _globalUsersRepositoryMock = new Mock<IGlobalUsersRepository>();
            _loggerMock = new Mock<ILogger<GlobalUsersController>>();

            _sut = new GlobalUsersController(
              _globalUsersRepositoryMock.Object,
              _loggerMock.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WhenRepositoryIsNull_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
              new GlobalUsersController(null!, _loggerMock.Object));
        }

        [Fact]
        public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
              new GlobalUsersController(_globalUsersRepositoryMock.Object, null!));
        }

        #endregion

        #region RegisterOrLogin Tests

        [Fact]
        public async Task RegisterOrLogin_WhenNameIsEmpty_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new LoginRequest
            {
                Name = "",
                DeviceFingerprint = "fingerprint123",
                DeviceInfo = "Device Info"
            };

            // Act
            var result = await _sut.RegisterOrLogin(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            badRequestResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task RegisterOrLogin_WhenNameIsNull_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new LoginRequest
            {
                Name = null!,
                DeviceFingerprint = "fingerprint123",
                DeviceInfo = "Device Info"
            };

            // Act
            var result = await _sut.RegisterOrLogin(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            badRequestResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task RegisterOrLogin_WhenDeviceFingerprintIsEmpty_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new LoginRequest
            {
                Name = "TestUser",
                DeviceFingerprint = "",
                DeviceInfo = "Device Info"
            };

            // Act
            var result = await _sut.RegisterOrLogin(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            badRequestResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task RegisterOrLogin_WhenDeviceFingerprintIsNull_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new LoginRequest
            {
                Name = "TestUser",
                DeviceFingerprint = null!,
                DeviceInfo = "Device Info"
            };

            // Act
            var result = await _sut.RegisterOrLogin(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            badRequestResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task RegisterOrLogin_WhenUserExistsWithSameFingerprint_ShouldReturnOkWithIsNewFalse()
        {
            // Arrange
            var request = new LoginRequest
            {
                Name = "ExistingUser",
                DeviceFingerprint = "fingerprint123",
                DeviceInfo = "Device Info"
            };
            var existingUser = new GlobalUser
            {
                Id = 1,
                Name = "ExistingUser",
                DeviceFingerprint = "fingerprint123",
                DeviceInfo = "Device Info",
                LastSeenAt = DateTime.UtcNow
            };

            _globalUsersRepositoryMock
              .Setup(x => x.GetByNameAsync(request.Name))
              .ReturnsAsync(existingUser);
            _globalUsersRepositoryMock
              .Setup(x => x.UpdateAsync(existingUser.Id, It.IsAny<GlobalUser>()))
              .ReturnsAsync(existingUser);

            // Act
            var result = await _sut.RegisterOrLogin(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.StatusCode.Should().Be(200);
            var response = Assert.IsType<GlobalUserResponse>(okResult.Value);
            response.Id.Should().Be(1);
            response.Name.Should().Be("ExistingUser");
            response.IsNew.Should().BeFalse();
            _globalUsersRepositoryMock.Verify(x => x.UpdateAsync(existingUser.Id, It.IsAny<GlobalUser>()), Times.Once);
        }

        [Fact]
        public async Task RegisterOrLogin_WhenUserExistsWithDifferentFingerprint_ShouldReturnForbidden()
        {
            // Arrange
            var request = new LoginRequest
            {
                Name = "ExistingUser",
                DeviceFingerprint = "differentFingerprint",
                DeviceInfo = "Device Info"
            };
            var existingUser = new GlobalUser
            {
                Id = 1,
                Name = "ExistingUser",
                DeviceFingerprint = "fingerprint123",
                DeviceInfo = "Device Info",
                LastSeenAt = DateTime.UtcNow
            };

            _globalUsersRepositoryMock
              .Setup(x => x.GetByNameAsync(request.Name))
              .ReturnsAsync(existingUser);

            // Act
            var result = await _sut.RegisterOrLogin(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            statusCodeResult.StatusCode.Should().Be(403);
        }

        [Fact]
        public async Task RegisterOrLogin_WhenUserDoesNotExist_ShouldCreateNewUserAndReturnIsNewTrue()
        {
            // Arrange
            var request = new LoginRequest
            {
                Name = "NewUser",
                DeviceFingerprint = "fingerprint123",
                DeviceInfo = "Device Info"
            };
            var newUser = new GlobalUser
            {
                Id = 2,
                Name = "NewUser",
                DeviceFingerprint = "fingerprint123",
                DeviceInfo = "Device Info",
                LastSeenAt = DateTime.UtcNow
            };

            _globalUsersRepositoryMock
              .Setup(x => x.GetByNameAsync(request.Name))
              .ReturnsAsync((GlobalUser?)null);
            _globalUsersRepositoryMock
              .Setup(x => x.CreateAsync(It.IsAny<GlobalUser>()))
              .ReturnsAsync(newUser);

            // Act
            var result = await _sut.RegisterOrLogin(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.StatusCode.Should().Be(200);
            var response = Assert.IsType<GlobalUserResponse>(okResult.Value);
            response.Id.Should().Be(2);
            response.Name.Should().Be("NewUser");
            response.IsNew.Should().BeTrue();
            _globalUsersRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<GlobalUser>()), Times.Once);
        }

        [Fact]
        public async Task RegisterOrLogin_WhenDeviceInfoIsNull_ShouldDefaultToUnknownDevice()
        {
            // Arrange
            var request = new LoginRequest
            {
                Name = "NewUser",
                DeviceFingerprint = "fingerprint123",
                DeviceInfo = null
            };
            var newUser = new GlobalUser
            {
                Id = 2,
                Name = "NewUser",
                DeviceFingerprint = "fingerprint123",
                DeviceInfo = "Unknown Device",
                LastSeenAt = DateTime.UtcNow
            };

            _globalUsersRepositoryMock
              .Setup(x => x.GetByNameAsync(request.Name))
              .ReturnsAsync((GlobalUser?)null);
            _globalUsersRepositoryMock
              .Setup(x => x.CreateAsync(It.IsAny<GlobalUser>()))
              .ReturnsAsync(newUser);

            // Act
            var result = await _sut.RegisterOrLogin(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.StatusCode.Should().Be(200);
            var response = Assert.IsType<GlobalUserResponse>(okResult.Value);
            response.IsNew.Should().BeTrue();
        }

        #endregion

        #region GetById Tests

        [Fact]
        public async Task GetById_WhenUserExists_ShouldReturnOkWithUser()
        {
            // Arrange
            int userId = 1;
            var user = new GlobalUser
            {
                Id = userId,
                Name = "TestUser",
                DeviceFingerprint = "fingerprint123",
                DeviceInfo = "Device Info",
                LastSeenAt = DateTime.UtcNow
            };

            _globalUsersRepositoryMock
              .Setup(x => x.GetByIdAsync(userId))
              .ReturnsAsync(user);

            // Act
            var result = await _sut.GetById(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.StatusCode.Should().Be(200);
            var returnedUser = Assert.IsType<GlobalUser>(okResult.Value);
            returnedUser.Id.Should().Be(userId);
            returnedUser.Name.Should().Be("TestUser");
        }

        [Fact]
        public async Task GetById_WhenUserDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            int userId = 999;

            _globalUsersRepositoryMock
              .Setup(x => x.GetByIdAsync(userId))
              .ReturnsAsync((GlobalUser?)null);

            // Act
            var result = await _sut.GetById(userId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            notFoundResult.StatusCode.Should().Be(404);
        }

        #endregion
    }
}
