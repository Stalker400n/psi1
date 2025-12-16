using FluentAssertions;
using back.Models;
using back.Data;
using back.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace back.Tests.Repositories
{
    public class GlobalUsersRepositoryTests : IAsyncLifetime
    {
        private ApplicationDbContext _context = null!;
        private GlobalUsersRepository _sut = null!;

        public async Task InitializeAsync()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
              .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
              .Options;

            _context = new ApplicationDbContext(options);
            await _context.Database.EnsureCreatedAsync();
            _sut = new GlobalUsersRepository(_context);
        }

        public async Task DisposeAsync()
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.DisposeAsync();
        }

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WhenValidUser_ShouldCreateAndReturnUser()
        {
            // Arrange
            var user = new GlobalUser
            {
                Name = "Global User",
                DeviceFingerprint = "fingerprint123",
                DeviceInfo = "Test Device",
                LastSeenAt = DateTime.UtcNow
            };

            // Act
            var result = await _sut.CreateAsync(user);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.Name.Should().Be("Global User");
            result.DeviceFingerprint.Should().Be("fingerprint123");

            // Verify in database
            var dbUser = await _context.GlobalUsers.FirstOrDefaultAsync(u => u.Id == result.Id);
            dbUser.Should().NotBeNull();
            dbUser!.Name.Should().Be("Global User");
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WhenUserExists_ShouldReturnUser()
        {
            // Arrange
            var user = new GlobalUser
            {
                Name = "Test User",
                DeviceFingerprint = "fingerprint123",
                DeviceInfo = "Test Device",
                LastSeenAt = DateTime.UtcNow
            };
            var createdUser = await _sut.CreateAsync(user);

            // Act
            var result = await _sut.GetByIdAsync(createdUser.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(createdUser.Id);
            result.Name.Should().Be("Test User");
        }

        [Fact]
        public async Task GetByIdAsync_WhenUserNotFound_ShouldReturnNull()
        {
            // Act
            var result = await _sut.GetByIdAsync(9999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetByNameAsync Tests

        [Fact]
        public async Task GetByNameAsync_WhenUserExists_ShouldReturnUser()
        {
            // Arrange
            var user = new GlobalUser
            {
                Name = "UniqueUser",
                DeviceFingerprint = "fingerprint123",
                DeviceInfo = "Test Device",
                LastSeenAt = DateTime.UtcNow
            };
            var createdUser = await _sut.CreateAsync(user);

            // Act
            var result = await _sut.GetByNameAsync("UniqueUser");

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(createdUser.Id);
            result.Name.Should().Be("UniqueUser");
        }

        [Fact]
        public async Task GetByNameAsync_WhenUserNotFound_ShouldReturnNull()
        {
            // Act
            var result = await _sut.GetByNameAsync("NonExistentUser");

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WhenUserExists_ShouldUpdateAndReturnUser()
        {
            // Arrange
            var user = new GlobalUser
            {
                Name = "Original",
                DeviceFingerprint = "fingerprint123",
                DeviceInfo = "Test Device",
                LastSeenAt = DateTime.UtcNow
            };
            var createdUser = await _sut.CreateAsync(user);

            var updatedUser = new GlobalUser
            {
                Id = createdUser.Id,
                Name = "Updated",
                DeviceFingerprint = "fingerprint456",
                DeviceInfo = "Updated Device",
                LastSeenAt = DateTime.UtcNow
            };

            // Act
            var result = await _sut.UpdateAsync(createdUser.Id, updatedUser);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Updated");
            result.DeviceFingerprint.Should().Be("fingerprint456");

            // Verify in database
            var dbUser = await _context.GlobalUsers.FirstOrDefaultAsync(u => u.Id == createdUser.Id);
            dbUser!.Name.Should().Be("Updated");
        }

        [Fact]
        public async Task UpdateAsync_WhenUserNotFound_ShouldReturnNull()
        {
            // Arrange
            var user = new GlobalUser
            {
                Name = "User",
                DeviceFingerprint = "fingerprint123",
                DeviceInfo = "Test Device",
                LastSeenAt = DateTime.UtcNow
            };

            // Act
            var result = await _sut.UpdateAsync(9999, user);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WhenUserExists_ShouldDeleteUser()
        {
            // Arrange
            var user = new GlobalUser
            {
                Name = "User to Delete",
                DeviceFingerprint = "fingerprint123",
                DeviceInfo = "Test Device",
                LastSeenAt = DateTime.UtcNow
            };
            var createdUser = await _sut.CreateAsync(user);

            // Act
            var result = await _sut.DeleteAsync(createdUser.Id);

            // Assert
            result.Should().BeTrue();

            // Verify in database
            var dbUser = await _context.GlobalUsers.FirstOrDefaultAsync(u => u.Id == createdUser.Id);
            dbUser.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_WhenUserNotFound_ShouldReturnFalse()
        {
            // Act
            var result = await _sut.DeleteAsync(9999);

            // Assert
            result.Should().BeFalse();
        }

        #endregion
    }
}
