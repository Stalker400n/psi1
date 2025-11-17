using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using back.Controllers;
using back.Models;
using back.Models.Enums;
using back.Data.Repositories;
using back.Exceptions;

namespace back.Tests.Controllers
{
  public class UsersControllerTests
  {
    private readonly Mock<IUsersRepository> _usersRepositoryMock;
    private readonly UsersController _sut;

    public UsersControllerTests()
    {
      _usersRepositoryMock = new Mock<IUsersRepository>();
      _sut = new UsersController(_usersRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WhenUsersRepositoryIsNull_ShouldThrowArgumentNullException()
    {
      // Act & Assert
      Assert.Throws<ArgumentNullException>(() => new UsersController(null!));
    }

    #endregion

    #region GetUsers Tests

    [Fact]
    public async Task GetUsers_WhenUsersExist_ShouldReturnOkWithUsers()
    {
      // Arrange
      var teamId = 1;
      var users = new List<User>
      {
        new User { Id = 1, Name = "User 1", Role = Role.Member },
        new User { Id = 2, Name = "User 2", Role = Role.Owner }
      };

      _usersRepositoryMock.Setup(x => x.GetUsersAsync(teamId))
        .ReturnsAsync(users);

      // Act
      var result = await _sut.GetUsers(teamId);

      // Assert
      var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
      var returnedUsers = okResult.Value.Should().BeAssignableTo<IEnumerable<User>>().Subject;
      returnedUsers.Should().HaveCount(2);
      returnedUsers.Should().BeEquivalentTo(users);
    }

    [Fact]
    public async Task GetUsers_WhenTeamNotFound_ShouldReturnNotFound()
    {
      // Arrange
      var teamId = 999;
      _usersRepositoryMock.Setup(x => x.GetUsersAsync(teamId))
        .ReturnsAsync((IEnumerable<User>?)null);

      // Act
      var result = await _sut.GetUsers(teamId);

      // Assert
      var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
      notFoundResult.Value.Should().BeEquivalentTo(new { message = "Team not found" });
    }

    [Fact]
    public async Task GetUsers_WhenNoUsers_ShouldReturnOkWithEmptyList()
    {
      // Arrange
      var teamId = 1;
      var users = new List<User>();

      _usersRepositoryMock.Setup(x => x.GetUsersAsync(teamId))
        .ReturnsAsync(users);

      // Act
      var result = await _sut.GetUsers(teamId);

      // Assert
      var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
      var returnedUsers = okResult.Value.Should().BeAssignableTo<IEnumerable<User>>().Subject;
      returnedUsers.Should().BeEmpty();
    }

    #endregion

    #region GetUser Tests

    [Fact]
    public async Task GetUser_WhenUserExists_ShouldReturnOkWithUser()
    {
      // Arrange
      var teamId = 1;
      var userId = 1;
      var user = new User { Id = userId, Name = "User 1", Role = Role.Member };

      _usersRepositoryMock.Setup(x => x.GetUserAsync(teamId, userId))
        .ReturnsAsync(user);

      // Act
      var result = await _sut.GetUser(teamId, userId);

      // Assert
      var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
      var returnedUser = okResult.Value.Should().BeOfType<User>().Subject;
      returnedUser.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task GetUser_WhenUserNotFound_ShouldReturnNotFound()
    {
      // Arrange
      var teamId = 1;
      var userId = 999;

      _usersRepositoryMock.Setup(x => x.GetUserAsync(teamId, userId))
        .ReturnsAsync((User?)null);

      // Act
      var result = await _sut.GetUser(teamId, userId);

      // Assert
      var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
      notFoundResult.Value.Should().BeEquivalentTo(new { message = "Team or user not found" });
    }

    [Fact]
    public async Task GetUser_WhenTeamNotFound_ShouldReturnNotFound()
    {
      // Arrange
      var teamId = 999;
      var userId = 1;

      _usersRepositoryMock.Setup(x => x.GetUserAsync(teamId, userId))
        .ReturnsAsync((User?)null);

      // Act
      var result = await _sut.GetUser(teamId, userId);

      // Assert
      var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
      notFoundResult.Value.Should().BeEquivalentTo(new { message = "Team or user not found" });
    }

    #endregion

    #region AddUser Tests

    [Fact]
    public async Task AddUser_WhenValidUser_ShouldReturnCreatedAtActionWithUser()
    {
      // Arrange
      var teamId = 1;
      var user = new User { Name = "New User", Role = Role.Member };
      var createdUser = new User { Id = 1, Name = "New User", Role = Role.Member };

      _usersRepositoryMock.Setup(x => x.CreateUserAsync(teamId, user))
        .ReturnsAsync(createdUser);

      // Act
      var result = await _sut.AddUser(teamId, user);

      // Assert
      var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
      createdResult.ActionName.Should().Be(nameof(UsersController.GetUser));
      createdResult.RouteValues.Should().ContainKey("teamId").WhoseValue.Should().Be(teamId);
      createdResult.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(createdUser.Id);
      
      var returnedUser = createdResult.Value.Should().BeOfType<User>().Subject;
      returnedUser.Should().BeEquivalentTo(createdUser);
    }

    [Fact]
    public async Task AddUser_WhenTeamNotFound_ShouldReturnNotFound()
    {
      // Arrange
      var teamId = 999;
      var user = new User { Name = "New User", Role = Role.Member };

      _usersRepositoryMock.Setup(x => x.CreateUserAsync(teamId, user))
        .ReturnsAsync((User?)null);

      // Act
      var result = await _sut.AddUser(teamId, user);

      // Assert
      var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
      notFoundResult.Value.Should().BeEquivalentTo(new { message = "Team not found" });
    }

    #endregion

    #region UpdateUser Tests

    [Fact]
    public async Task UpdateUser_WhenValidUser_ShouldReturnOkWithUpdatedUser()
    {
      // Arrange
      var teamId = 1;
      var userId = 1;
      var user = new User { Name = "Updated User", Role = Role.Member };
      var updatedUser = new User { Id = userId, Name = "Updated User", Role = Role.Member };

      _usersRepositoryMock.Setup(x => x.UpdateUserAsync(teamId, userId, user))
        .ReturnsAsync(updatedUser);

      // Act
      var result = await _sut.UpdateUser(teamId, userId, user);

      // Assert
      var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
      var returnedUser = okResult.Value.Should().BeOfType<User>().Subject;
      returnedUser.Should().BeEquivalentTo(updatedUser);
    }

    [Fact]
    public async Task UpdateUser_WhenUserNotFound_ShouldReturnNotFound()
    {
      // Arrange
      var teamId = 1;
      var userId = 999;
      var user = new User { Name = "Updated User", Role = Role.Member };

      _usersRepositoryMock.Setup(x => x.UpdateUserAsync(teamId, userId, user))
        .ReturnsAsync((User?)null);

      // Act
      var result = await _sut.UpdateUser(teamId, userId, user);

      // Assert
      var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
      notFoundResult.Value.Should().BeEquivalentTo(new { message = "Team or user not found" });
    }

    [Fact]
    public async Task UpdateUser_WhenTeamNotFound_ShouldReturnNotFound()
    {
      // Arrange
      var teamId = 999;
      var userId = 1;
      var user = new User { Name = "Updated User", Role = Role.Member };

      _usersRepositoryMock.Setup(x => x.UpdateUserAsync(teamId, userId, user))
        .ReturnsAsync((User?)null);

      // Act
      var result = await _sut.UpdateUser(teamId, userId, user);

      // Assert
      var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
      notFoundResult.Value.Should().BeEquivalentTo(new { message = "Team or user not found" });
    }

    #endregion

    #region ChangeUserRole Tests

    [Fact]
    public async Task ChangeUserRole_WhenValid_ShouldReturnOkWithUpdatedUser()
    {
      // Arrange
      var teamId = 1;
      var userId = 1;
      var request = new RoleChangeRequest { Role = Role.Owner, RequestingUserId = 2 };
      var updatedUser = new User { Id = userId, Name = "User", Role = Role.Owner };

      _usersRepositoryMock.Setup(x => x.ChangeUserRoleAsync(teamId, userId, request.Role, request.RequestingUserId))
        .ReturnsAsync(updatedUser);

      // Act
      var result = await _sut.ChangeUserRole(teamId, userId, request);

      // Assert
      var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
      var returnedUser = okResult.Value.Should().BeOfType<User>().Subject;
      returnedUser.Should().BeEquivalentTo(updatedUser);
    }

    [Fact]
    public async Task ChangeUserRole_WhenUserNotFound_ShouldReturnNotFound()
    {
      // Arrange
      var teamId = 1;
      var userId = 999;
      var request = new RoleChangeRequest { Role = Role.Owner, RequestingUserId = 2 };

      _usersRepositoryMock.Setup(x => x.ChangeUserRoleAsync(teamId, userId, request.Role, request.RequestingUserId))
        .ReturnsAsync((User?)null);

      // Act
      var result = await _sut.ChangeUserRole(teamId, userId, request);

      // Assert
      var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
      notFoundResult.Value.Should().BeEquivalentTo(new { message = "Team or user not found" });
    }

    [Fact]
    public async Task ChangeUserRole_WhenRoleChangeNotAllowed_ShouldReturnBadRequest()
    {
      // Arrange
      var teamId = 1;
      var userId = 1;
      var request = new RoleChangeRequest { Role = Role.Owner, RequestingUserId = 2 };
      var errorMessage = "Only admins can change user roles";

      _usersRepositoryMock.Setup(x => x.ChangeUserRoleAsync(teamId, userId, request.Role, request.RequestingUserId))
        .ThrowsAsync(new RoleChangeException(errorMessage));

      // Act
      var result = await _sut.ChangeUserRole(teamId, userId, request);

      // Assert
      var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
      badRequestResult.Value.Should().BeEquivalentTo(new { message = errorMessage });
    }

    [Fact]
    public async Task ChangeUserRole_WhenLastAdminDemotion_ShouldReturnBadRequest()
    {
      // Arrange
      var teamId = 1;
      var userId = 1;
      var request = new RoleChangeRequest { Role = Role.Member, RequestingUserId = 1 };
      var errorMessage = "Cannot demote the last admin";

      _usersRepositoryMock.Setup(x => x.ChangeUserRoleAsync(teamId, userId, request.Role, request.RequestingUserId))
        .ThrowsAsync(new RoleChangeException(errorMessage));

      // Act
      var result = await _sut.ChangeUserRole(teamId, userId, request);

      // Assert
      var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
      badRequestResult.Value.Should().BeEquivalentTo(new { message = errorMessage });
    }

    #endregion

    #region DeleteUser Tests

    [Fact]
    public async Task DeleteUser_WhenUserExists_ShouldReturnNoContent()
    {
      // Arrange
      var teamId = 1;
      var userId = 1;

      _usersRepositoryMock.Setup(x => x.DeleteUserAsync(teamId, userId))
        .ReturnsAsync(true);

      // Act
      var result = await _sut.DeleteUser(teamId, userId);

      // Assert
      result.Should().BeOfType<NoContentResult>();
      _usersRepositoryMock.Verify(x => x.DeleteUserAsync(teamId, userId), Times.Once);
    }

    [Fact]
    public async Task DeleteUser_WhenUserNotFound_ShouldReturnNotFound()
    {
      // Arrange
      var teamId = 1;
      var userId = 999;

      _usersRepositoryMock.Setup(x => x.DeleteUserAsync(teamId, userId))
        .ReturnsAsync(false);

      // Act
      var result = await _sut.DeleteUser(teamId, userId);

      // Assert
      var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
      notFoundResult.Value.Should().BeEquivalentTo(new { message = "Team or user not found" });
    }

    [Fact]
    public async Task DeleteUser_WhenTeamNotFound_ShouldReturnNotFound()
    {
      // Arrange
      var teamId = 999;
      var userId = 1;

      _usersRepositoryMock.Setup(x => x.DeleteUserAsync(teamId, userId))
        .ReturnsAsync(false);

      // Act
      var result = await _sut.DeleteUser(teamId, userId);

      // Assert
      var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
      notFoundResult.Value.Should().BeEquivalentTo(new { message = "Team or user not found" });
    }

    #endregion

    #region Repository Verification Tests

    [Fact]
    public async Task GetUsers_ShouldCallRepositoryWithCorrectTeamId()
    {
      // Arrange
      var teamId = 42;
      _usersRepositoryMock.Setup(x => x.GetUsersAsync(teamId))
        .ReturnsAsync(new List<User>());

      // Act
      await _sut.GetUsers(teamId);

      // Assert
      _usersRepositoryMock.Verify(x => x.GetUsersAsync(teamId), Times.Once);
    }

    [Fact]
    public async Task GetUser_ShouldCallRepositoryWithCorrectParameters()
    {
      // Arrange
      var teamId = 42;
      var userId = 10;
      _usersRepositoryMock.Setup(x => x.GetUserAsync(teamId, userId))
        .ReturnsAsync(new User());

      // Act
      await _sut.GetUser(teamId, userId);

      // Assert
      _usersRepositoryMock.Verify(x => x.GetUserAsync(teamId, userId), Times.Once);
    }

    [Fact]
    public async Task AddUser_ShouldCallRepositoryWithCorrectParameters()
    {
      // Arrange
      var teamId = 42;
      var user = new User { Name = "Test User" };
      _usersRepositoryMock.Setup(x => x.CreateUserAsync(teamId, user))
        .ReturnsAsync(new User { Id = 1 });

      // Act
      await _sut.AddUser(teamId, user);

      // Assert
      _usersRepositoryMock.Verify(x => x.CreateUserAsync(teamId, user), Times.Once);
    }

    [Fact]
    public async Task UpdateUser_ShouldCallRepositoryWithCorrectParameters()
    {
      // Arrange
      var teamId = 42;
      var userId = 10;
      var user = new User { Name = "Updated User" };
      _usersRepositoryMock.Setup(x => x.UpdateUserAsync(teamId, userId, user))
        .ReturnsAsync(new User { Id = userId });

      // Act
      await _sut.UpdateUser(teamId, userId, user);

      // Assert
      _usersRepositoryMock.Verify(x => x.UpdateUserAsync(teamId, userId, user), Times.Once);
    }

    [Fact]
    public async Task ChangeUserRole_ShouldCallRepositoryWithCorrectParameters()
    {
      // Arrange
      var teamId = 42;
      var userId = 10;
      var request = new RoleChangeRequest { Role = Role.Owner, RequestingUserId = 20 };
      _usersRepositoryMock.Setup(x => x.ChangeUserRoleAsync(teamId, userId, request.Role, request.RequestingUserId))
        .ReturnsAsync(new User { Id = userId });

      // Act
      await _sut.ChangeUserRole(teamId, userId, request);

      // Assert
      _usersRepositoryMock.Verify(x => x.ChangeUserRoleAsync(teamId, userId, request.Role, request.RequestingUserId), Times.Once);
    }

    #endregion
  }
}