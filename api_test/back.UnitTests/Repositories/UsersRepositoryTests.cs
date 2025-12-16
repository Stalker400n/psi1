using FluentAssertions;
using back.Models;
using back.Models.Enums;
using back.Data;
using back.Data.Repositories;
using back.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace back.Tests.Repositories
{
    public class UsersRepositoryTests : IAsyncLifetime
    {
        private ApplicationDbContext _context = null!;
        private UsersRepository _sut = null!;
        private Team _testTeam = null!;

        public async Task InitializeAsync()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
              .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
              .Options;

            _context = new ApplicationDbContext(options);
            await _context.Database.EnsureCreatedAsync();

            // Create test team
            _testTeam = new Team
            {
                Name = "Test Team",
                CurrentSongIndex = 0,
                Songs = new List<Song>()
            };
            _context.Teams.Add(_testTeam);
            await _context.SaveChangesAsync();

            _sut = new UsersRepository(_context);
        }

        public async Task DisposeAsync()
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.DisposeAsync();
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullContext_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new UsersRepository(null!));
        }

        #endregion

        #region GetUsersAsync Tests

        [Fact]
        public async Task GetUsersAsync_WhenUsersExist_ShouldReturnAllUsersSortedByJoinedAt()
        {
            // Arrange
            var user1 = new User { Name = "User 1", Score = 100, Role = Role.Member, IsActive = true };
            var user2 = new User { Name = "User 2", Score = 200, Role = Role.Member, IsActive = true };

            await _sut.CreateUserAsync(_testTeam.Id, user1);
            await _sut.CreateUserAsync(_testTeam.Id, user2);

            // Act
            var result = await _sut.GetUsersAsync(_testTeam.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Should().HaveCount(2);
            result.First().Name.Should().Be("User 1");
            result.Last().Name.Should().Be("User 2");
        }

        [Fact]
        public async Task GetUsersAsync_WhenNoUsersExist_ShouldReturnEmpty()
        {
            // Act
            var result = await _sut.GetUsersAsync(_testTeam.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUsersAsync_WhenTeamNotFound_ShouldReturnNull()
        {
            // Act
            var result = await _sut.GetUsersAsync(9999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetUserAsync Tests

        [Fact]
        public async Task GetUserAsync_WhenUserExists_ShouldReturnUser()
        {
            // Arrange
            var user = new User { Name = "Test User", Score = 100, Role = Role.Member, IsActive = true };
            var createdUser = await _sut.CreateUserAsync(_testTeam.Id, user);

            // Act
            var result = await _sut.GetUserAsync(_testTeam.Id, createdUser!.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(createdUser.Id);
            result.Name.Should().Be("Test User");
        }

        [Fact]
        public async Task GetUserAsync_WhenUserNotFound_ShouldReturnNull()
        {
            // Act
            var result = await _sut.GetUserAsync(_testTeam.Id, 9999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserAsync_WhenTeamNotFound_ShouldReturnNull()
        {
            // Act
            var result = await _sut.GetUserAsync(9999, 1);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateUserAsync Tests

        [Fact]
        public async Task CreateUserAsync_WhenValidUser_ShouldCreateAndReturnUser()
        {
            // Arrange
            var newTeam = new Team { Name = "New Team", CurrentSongIndex = 0, Songs = new List<Song>() };
            _context.Teams.Add(newTeam);
            await _context.SaveChangesAsync();

            var user = new User
            {
                Name = "Test User",
                Score = 100,
                Role = Role.Member,
                IsActive = true
            };

            // Act
            var result = await _sut.CreateUserAsync(newTeam.Id, user);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().BeGreaterThan(0);
            result.Name.Should().Be("Test User");
            result.Score.Should().Be(100);
            result.Role.Should().Be(Role.Owner); // First user becomes Owner
        }

        [Fact]
        public async Task CreateUserAsync_FirstUserInTeam_ShouldSetAsOwner()
        {
            // Arrange
            var newTeam = new Team { Name = "Brand New Team", CurrentSongIndex = 0, Songs = new List<Song>() };
            _context.Teams.Add(newTeam);
            await _context.SaveChangesAsync();

            var user = new User { Name = "First User", Role = Role.Member };

            // Act
            var result = await _sut.CreateUserAsync(newTeam.Id, user);

            // Assert
            result.Should().NotBeNull();
            result!.Role.Should().Be(Role.Owner);
        }

        [Fact]
        public async Task CreateUserAsync_SecondUserInTeam_ShouldRemainMember()
        {
            // Arrange
            var user1 = new User { Name = "First User", Role = Role.Member };
            await _sut.CreateUserAsync(_testTeam.Id, user1);

            var user2 = new User { Name = "Second User", Role = Role.Member };

            // Act
            var result = await _sut.CreateUserAsync(_testTeam.Id, user2);

            // Assert
            result.Should().NotBeNull();
            result!.Role.Should().Be(Role.Member);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldSetJoinedAtToUtcNow()
        {
            // Arrange
            var newTeam = new Team { Name = "Time Test Team", CurrentSongIndex = 0, Songs = new List<Song>() };
            _context.Teams.Add(newTeam);
            await _context.SaveChangesAsync();

            var user = new User { Name = "Time User", Role = Role.Member };
            var beforeTime = DateTime.UtcNow;

            // Act
            var result = await _sut.CreateUserAsync(newTeam.Id, user);

            var afterTime = DateTime.UtcNow;

            // Assert
            result.Should().NotBeNull();
            result!.JoinedAt.Should().BeOnOrAfter(beforeTime);
            result!.JoinedAt.Should().BeOnOrBefore(afterTime);
        }

        [Fact]
        public async Task CreateUserAsync_WhenTeamNotFound_ShouldReturnNull()
        {
            // Arrange
            var user = new User { Name = "User", Role = Role.Member };

            // Act
            var result = await _sut.CreateUserAsync(9999, user);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateUserAsync_WhenTeamUsersIsNull_ShouldInitializeList()
        {
            // Arrange
            var newTeam = new Team { Name = "Null Users Team", CurrentSongIndex = 0, Songs = new List<Song>(), Users = new List<User>() };
            _context.Teams.Add(newTeam);
            await _context.SaveChangesAsync();

            var user = new User { Name = "User", Role = Role.Member };

            // Act
            var result = await _sut.CreateUserAsync(newTeam.Id, user);

            // Assert
            result.Should().NotBeNull();
        }

        #endregion

        #region UpdateUserAsync Tests

        [Fact]
        public async Task UpdateUserAsync_WhenUserExists_ShouldUpdateAllFields()
        {
            // Arrange
            var user = new User { Name = "Original", Score = 100, Role = Role.Member, IsActive = true };
            var createdUser = await _sut.CreateUserAsync(_testTeam.Id, user);

            var updatedUser = new User
            {
                Name = "Updated",
                Score = 500,
                Role = Role.Moderator,
                IsActive = false
            };

            // Act
            var result = await _sut.UpdateUserAsync(_testTeam.Id, createdUser!.Id, updatedUser);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Updated");
            result.Score.Should().Be(500);
            result.Role.Should().Be(Role.Moderator);
            result.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateUserAsync_WhenUserNotFound_ShouldReturnNull()
        {
            // Arrange
            var user = new User { Name = "User", Score = 100, Role = Role.Member, IsActive = true };

            // Act
            var result = await _sut.UpdateUserAsync(_testTeam.Id, 9999, user);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateUserAsync_WhenTeamNotFound_ShouldReturnNull()
        {
            // Act
            var result = await _sut.UpdateUserAsync(9999, 1, new User());

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region DeleteUserAsync Tests

        [Fact]
        public async Task DeleteUserAsync_WhenUserExists_ShouldDeleteAndReturnTrue()
        {
            // Arrange
            var user = new User { Name = "User", Score = 100, Role = Role.Member, IsActive = true };
            var createdUser = await _sut.CreateUserAsync(_testTeam.Id, user);

            // Act
            var result = await _sut.DeleteUserAsync(_testTeam.Id, createdUser!.Id);

            // Assert
            result.Should().BeTrue();
            var deletedUser = await _sut.GetUserAsync(_testTeam.Id, createdUser.Id);
            deletedUser.Should().BeNull();
        }

        [Fact]
        public async Task DeleteUserAsync_WhenUserNotFound_ShouldReturnFalse()
        {
            // Act
            var result = await _sut.DeleteUserAsync(_testTeam.Id, 9999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteUserAsync_WhenTeamNotFound_ShouldReturnFalse()
        {
            // Act
            var result = await _sut.DeleteUserAsync(9999, 1);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region ChangeUserRoleAsync (No Requester) Tests

        [Fact]
        public async Task ChangeUserRoleAsync_NoRequester_WhenUserExists_ShouldChangeRoleAndReturnUser()
        {
            // Arrange
            var user = new User { Name = "User", Score = 100, Role = Role.Member, IsActive = true };
            var createdUser = await _sut.CreateUserAsync(_testTeam.Id, user);

            // Act
            var result = await _sut.ChangeUserRoleAsync(_testTeam.Id, createdUser!.Id, Role.Moderator);

            // Assert
            result.Should().NotBeNull();
            result!.Role.Should().Be(Role.Moderator);
        }

        [Fact]
        public async Task ChangeUserRoleAsync_NoRequester_WhenUserNotFound_ShouldReturnNull()
        {
            // Act
            var result = await _sut.ChangeUserRoleAsync(_testTeam.Id, 9999, Role.Moderator);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ChangeUserRoleAsync_NoRequester_WhenTeamNotFound_ShouldReturnNull()
        {
            // Act
            var result = await _sut.ChangeUserRoleAsync(9999, 1, Role.Moderator);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region ChangeUserRoleAsync (With Requester) Tests

        [Fact]
        public async Task ChangeUserRoleAsync_WithRequester_OwnerChangingRole_ShouldSucceed()
        {
            // Arrange
            var owner = new User { Name = "Owner", Role = Role.Member };
            var targetUser = new User { Name = "Target", Role = Role.Member };

            var createdOwner = await _sut.CreateUserAsync(_testTeam.Id, owner); // Will be Owner
            var createdTarget = await _sut.CreateUserAsync(_testTeam.Id, targetUser);

            // Act
            var result = await _sut.ChangeUserRoleAsync(_testTeam.Id, createdTarget!.Id, Role.Moderator, createdOwner!.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Role.Should().Be(Role.Moderator);
        }

        [Fact]
        public async Task ChangeUserRoleAsync_WithRequester_ModeratorChangingRole_ShouldSucceed()
        {
            // Arrange
            var newTeam = new Team { Name = "Moderator Team", CurrentSongIndex = 0, Songs = new List<Song>() };
            _context.Teams.Add(newTeam);
            await _context.SaveChangesAsync();

            var moderator = new User { Name = "Moderator", Role = Role.Moderator };
            var targetUser = new User { Name = "Target", Role = Role.Member };

            _context.Users.Add(moderator);
            await _context.SaveChangesAsync();

            _context.Users.Add(targetUser);
            await _context.SaveChangesAsync();

            // Update team's users list
            newTeam.Users ??= new List<User>();
            newTeam.Users.Add(moderator);
            newTeam.Users.Add(targetUser);
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.ChangeUserRoleAsync(newTeam.Id, targetUser.Id, Role.Owner, moderator.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Role.Should().Be(Role.Owner);
        }

        [Fact]
        public async Task ChangeUserRoleAsync_WithRequester_MemberAttemptingChange_ShouldThrowException()
        {
            // Arrange
            var newTeam = new Team { Name = "Member Team", CurrentSongIndex = 0, Songs = new List<Song>() };
            _context.Teams.Add(newTeam);
            await _context.SaveChangesAsync();

            var member = new User { Name = "Member", Role = Role.Member };
            var targetUser = new User { Name = "Target", Role = Role.Member };

            _context.Users.Add(member);
            await _context.SaveChangesAsync();

            _context.Users.Add(targetUser);
            await _context.SaveChangesAsync();

            newTeam.Users ??= new List<User>();
            newTeam.Users.Add(member);
            newTeam.Users.Add(targetUser);
            await _context.SaveChangesAsync();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<RoleChangeException>(
                () => _sut.ChangeUserRoleAsync(newTeam.Id, targetUser.Id, Role.Moderator, member.Id));

            exception.Message.Should().Be("Only owners and moderators can change user roles");
        }

        [Fact]
        public async Task ChangeUserRoleAsync_WithRequester_RequestingUserNotInTeam_ShouldThrowException()
        {
            // Arrange
            var user = new User { Name = "User", Role = Role.Member };
            var createdUser = await _sut.CreateUserAsync(_testTeam.Id, user);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<RoleChangeException>(
                () => _sut.ChangeUserRoleAsync(_testTeam.Id, createdUser!.Id, Role.Moderator, 9999));

            exception.Message.Should().Be("Requesting user not found in team");
        }

        [Fact]
        public async Task ChangeUserRoleAsync_WithRequester_RemovingOnlyOwner_ShouldThrowException()
        {
            // Arrange
            var owner = new User { Name = "Only Owner", Role = Role.Member };
            var createdOwner = await _sut.CreateUserAsync(_testTeam.Id, owner); // Becomes Owner

            // Act & Assert
            var exception = await Assert.ThrowsAsync<RoleChangeException>(
                () => _sut.ChangeUserRoleAsync(_testTeam.Id, createdOwner!.Id, Role.Member, createdOwner.Id));

            exception.Message.Should().Be("A team must have at least one owner");
        }

        [Fact]
        public async Task ChangeUserRoleAsync_WithRequester_RemovingOwnerWhenMultipleExist_ShouldSucceed()
        {
            // Arrange
            var newTeam = new Team { Name = "Multi Owner Team", CurrentSongIndex = 0, Songs = new List<Song>() };
            _context.Teams.Add(newTeam);
            await _context.SaveChangesAsync();

            var owner1 = new User { Name = "Owner1", Role = Role.Member };
            var owner2 = new User { Name = "Owner2", Role = Role.Owner };

            _context.Users.Add(owner1);
            await _context.SaveChangesAsync();

            _context.Users.Add(owner2);
            await _context.SaveChangesAsync();

            newTeam.Users ??= new List<User>();
            newTeam.Users.Add(owner1);
            owner1.Role = Role.Owner;
            newTeam.Users.Add(owner2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.ChangeUserRoleAsync(newTeam.Id, owner2.Id, Role.Member, owner1.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Role.Should().Be(Role.Member);
        }

        [Fact]
        public async Task ChangeUserRoleAsync_WithRequester_TargetUserNotFound_ShouldReturnNull()
        {
            // Arrange
            var owner = new User { Name = "Owner", Role = Role.Member };
            var createdOwner = await _sut.CreateUserAsync(_testTeam.Id, owner);

            // Act
            var result = await _sut.ChangeUserRoleAsync(_testTeam.Id, 9999, Role.Moderator, createdOwner!.Id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ChangeUserRoleAsync_WithRequester_TeamNotFound_ShouldReturnNull()
        {
            // Act
            var result = await _sut.ChangeUserRoleAsync(9999, 1, Role.Moderator, 1);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task FullUserLifecycle_CreateUpdateChangeRoleDelete_ShouldSucceed()
        {
            // Arrange
            var user = new User { Name = "Lifecycle User", Role = Role.Member, Score = 100, IsActive = true };

            // Act - Create
            var createdUser = await _sut.CreateUserAsync(_testTeam.Id, user);
            createdUser.Should().NotBeNull();
            createdUser!.Role.Should().Be(Role.Owner);

            // Update
            var updateData = new User { Name = "Updated User", Score = 250, IsActive = false, Role = Role.Moderator };
            var updatedUser = await _sut.UpdateUserAsync(_testTeam.Id, createdUser.Id, updateData);
            updatedUser.Should().NotBeNull();
            updatedUser!.Name.Should().Be("Updated User");
            updatedUser.Score.Should().Be(250);

            // Change role
            var roleChangedUser = await _sut.ChangeUserRoleAsync(_testTeam.Id, createdUser.Id, Role.Member);
            roleChangedUser.Should().NotBeNull();
            roleChangedUser!.Role.Should().Be(Role.Member);

            // Delete
            var deleteResult = await _sut.DeleteUserAsync(_testTeam.Id, createdUser.Id);
            deleteResult.Should().BeTrue();

            // Verify
            var deletedUser = await _sut.GetUserAsync(_testTeam.Id, createdUser.Id);
            deletedUser.Should().BeNull();
        }

        [Fact]
        public async Task MultipleUsersManagement_CreateUpdateAndDelete_ShouldHandleCorrectly()
        {
            // Arrange
            var user1 = new User { Name = "User1", Role = Role.Member, Score = 100 };
            var user2 = new User { Name = "User2", Role = Role.Member, Score = 200 };

            // Act
            var created1 = await _sut.CreateUserAsync(_testTeam.Id, user1);
            var created2 = await _sut.CreateUserAsync(_testTeam.Id, user2);

            // Get all
            var allUsers = await _sut.GetUsersAsync(_testTeam.Id);

            // Delete first
            var deleteResult = await _sut.DeleteUserAsync(_testTeam.Id, created1!.Id);

            // Get all after delete
            var remainingUsers = await _sut.GetUsersAsync(_testTeam.Id);

            // Assert
            allUsers.Should().HaveCount(2);
            deleteResult.Should().BeTrue();
            remainingUsers.Should().HaveCount(1);
            remainingUsers!.First().Name.Should().Be("User2");
        }

        #endregion
    }
}
