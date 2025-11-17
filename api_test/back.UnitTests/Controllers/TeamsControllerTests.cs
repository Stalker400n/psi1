using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using back.Controllers;
using back.Models;
using back.Data.Repositories;

namespace back.Tests.Controllers
{
    public class TeamsControllerTests
    {
        private readonly Mock<ITeamsRepository> _teamsRepositoryMock;
        private readonly TeamsController _sut;

        public TeamsControllerTests()
        {
            _teamsRepositoryMock = new Mock<ITeamsRepository>();
            _sut = new TeamsController(_teamsRepositoryMock.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WhenTeamsRepositoryIsNull_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
              new TeamsController(null!));
        }

        #endregion

        #region GetTeams Tests

        [Fact]
        public async Task GetTeams_ShouldReturnOkWithAllTeams()
        {
            // Arrange
            var teams = new List<Team>
      {
        new Team { Id = 1, Name = "Team 1", IsPrivate = false },
        new Team { Id = 2, Name = "Team 2", IsPrivate = true }
      };
            _teamsRepositoryMock
              .Setup(x => x.GetAllAsync())
              .ReturnsAsync(teams);

            // Act
            var result = await _sut.GetTeams();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.StatusCode.Should().Be(200);
            var returnedTeams = Assert.IsAssignableFrom<IEnumerable<Team>>(okResult.Value);
            returnedTeams.Should().HaveCount(2);
            _teamsRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetTeams_WithEmptyList_ShouldReturnOkWithEmptyTeams()
        {
            // Arrange
            var emptyTeams = new List<Team>();
            _teamsRepositoryMock
              .Setup(x => x.GetAllAsync())
              .ReturnsAsync(emptyTeams);

            // Act
            var result = await _sut.GetTeams();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.StatusCode.Should().Be(200);
            var returnedTeams = Assert.IsAssignableFrom<IEnumerable<Team>>(okResult.Value);
            returnedTeams.Should().BeEmpty();
        }

        #endregion

        #region GetTeam Tests

        [Fact]
        public async Task GetTeam_WithValidId_ShouldReturnOkWithTeam()
        {
            // Arrange
            int teamId = 1;
            var team = new Team
            {
                Id = teamId,
                Name = "Test Team",
                IsPrivate = false,
                Songs = new List<Song>(),
                Users = new List<User>(),
                Messages = new List<ChatMessage>()
            };
            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(teamId))
              .ReturnsAsync(team);

            // Act
            var result = await _sut.GetTeam(teamId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.StatusCode.Should().Be(200);
            var returnedTeam = Assert.IsType<Team>(okResult.Value);
            returnedTeam.Id.Should().Be(teamId);
            returnedTeam.Name.Should().Be("Test Team");
            _teamsRepositoryMock.Verify(x => x.GetByIdAsync(teamId), Times.Once);
        }

        [Fact]
        public async Task GetTeam_WhenTeamNotFound_ShouldReturnNotFound()
        {
            // Arrange
            int teamId = 999;
            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(teamId))
              .ReturnsAsync((Team?)null);

            // Act
            var result = await _sut.GetTeam(teamId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            notFoundResult.StatusCode.Should().Be(404);
        }

        #endregion

        #region CreateTeam Tests

        [Fact]
        public async Task CreateTeam_WithValidTeam_ShouldReturnCreatedAtAction()
        {
            // Arrange
            var teamToCreate = new Team
            {
                Id = 0,
                Name = "New Team",
                IsPrivate = false,
                CreatedByUserId = 1
            };
            var createdTeam = new Team
            {
                Id = 1,
                Name = "New Team",
                IsPrivate = false,
                CreatedByUserId = 1
            };
            _teamsRepositoryMock
              .Setup(x => x.CreateAsync(It.IsAny<Team>()))
              .ReturnsAsync(createdTeam);            // Act
            var result = await _sut.CreateTeam(teamToCreate);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            createdResult.StatusCode.Should().Be(201);
            createdResult.ActionName.Should().Be(nameof(TeamsController.GetTeam));
            createdResult.RouteValues!.Should().ContainKey("id");
            createdResult.RouteValues["id"].Should().Be(1);
            var returnedTeam = Assert.IsType<Team>(createdResult.Value);
            returnedTeam.Id.Should().Be(1);
            returnedTeam.Name.Should().Be("New Team");
            _teamsRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Team>()), Times.Once);
        }

        [Fact]
        public async Task CreateTeam_WithPrivateTeam_ShouldCreateAndReturnTeam()
        {
            // Arrange
            var teamToCreate = new Team
            {
                Id = 0,
                Name = "Private Team",
                IsPrivate = true,
                InviteCode = "ABC123",
                CreatedByUserId = 1
            };
            var createdTeam = new Team
            {
                Id = 2,
                Name = "Private Team",
                IsPrivate = true,
                InviteCode = "ABC123",
                CreatedByUserId = 1
            };
            _teamsRepositoryMock
              .Setup(x => x.CreateAsync(It.IsAny<Team>()))
              .ReturnsAsync(createdTeam);            // Act
            var result = await _sut.CreateTeam(teamToCreate);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            createdResult.StatusCode.Should().Be(201);
            var returnedTeam = Assert.IsType<Team>(createdResult.Value);
            returnedTeam.IsPrivate.Should().BeTrue();
            returnedTeam.InviteCode.Should().Be("ABC123");
        }

        #endregion

        #region UpdateTeam Tests

        [Fact]
        public async Task UpdateTeam_WithValidTeam_ShouldReturnOkWithUpdatedTeam()
        {
            // Arrange
            int teamId = 1;
            var updatedTeam = new Team
            {
                Id = teamId,
                Name = "Updated Team",
                IsPrivate = true
            };
            _teamsRepositoryMock
              .Setup(x => x.UpdateAsync(teamId, updatedTeam))
              .ReturnsAsync(updatedTeam);

            // Act
            var result = await _sut.UpdateTeam(teamId, updatedTeam);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.StatusCode.Should().Be(200);
            var returnedTeam = Assert.IsType<Team>(okResult.Value);
            returnedTeam.Name.Should().Be("Updated Team");
            returnedTeam.IsPrivate.Should().BeTrue();
            _teamsRepositoryMock.Verify(x => x.UpdateAsync(teamId, updatedTeam), Times.Once);
        }

        [Fact]
        public async Task UpdateTeam_WhenTeamNotFound_ShouldReturnNotFound()
        {
            // Arrange
            int teamId = 999;
            var updatedTeam = new Team
            {
                Id = teamId,
                Name = "Non-existent Team"
            };
            _teamsRepositoryMock
              .Setup(x => x.UpdateAsync(teamId, updatedTeam))
              .ReturnsAsync((Team?)null);

            // Act
            var result = await _sut.UpdateTeam(teamId, updatedTeam);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            notFoundResult.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task UpdateTeam_CanChangePrivacySettings()
        {
            // Arrange
            int teamId = 1;
            var updatedTeam = new Team
            {
                Id = teamId,
                Name = "Test Team",
                IsPrivate = true,
                InviteCode = "NEWINVITE"
            };
            _teamsRepositoryMock
              .Setup(x => x.UpdateAsync(teamId, updatedTeam))
              .ReturnsAsync(updatedTeam);

            // Act
            var result = await _sut.UpdateTeam(teamId, updatedTeam);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedTeam = Assert.IsType<Team>(okResult.Value);
            returnedTeam.IsPrivate.Should().BeTrue();
            returnedTeam.InviteCode.Should().Be("NEWINVITE");
        }

        #endregion

        #region DeleteTeam Tests

        [Fact]
        public async Task DeleteTeam_WithValidId_ShouldReturnNoContent()
        {
            // Arrange
            int teamId = 1;
            _teamsRepositoryMock
              .Setup(x => x.DeleteAsync(teamId))
              .ReturnsAsync(true);

            // Act
            var result = await _sut.DeleteTeam(teamId);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            noContentResult.StatusCode.Should().Be(204);
            _teamsRepositoryMock.Verify(x => x.DeleteAsync(teamId), Times.Once);
        }

        [Fact]
        public async Task DeleteTeam_WhenTeamNotFound_ShouldReturnNotFound()
        {
            // Arrange
            int teamId = 999;
            _teamsRepositoryMock
              .Setup(x => x.DeleteAsync(teamId))
              .ReturnsAsync(false);

            // Act
            var result = await _sut.DeleteTeam(teamId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            notFoundResult.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task DeleteTeam_MultipleTeams_ShouldDeleteEachCorrectly()
        {
            // Arrange
            int teamId1 = 1;
            int teamId2 = 2;
            _teamsRepositoryMock
              .Setup(x => x.DeleteAsync(teamId1))
              .ReturnsAsync(true);
            _teamsRepositoryMock
              .Setup(x => x.DeleteAsync(teamId2))
              .ReturnsAsync(true);

            // Act
            var result1 = await _sut.DeleteTeam(teamId1);
            var result2 = await _sut.DeleteTeam(teamId2);

            // Assert
            Assert.IsType<NoContentResult>(result1);
            Assert.IsType<NoContentResult>(result2);
            _teamsRepositoryMock.Verify(x => x.DeleteAsync(teamId1), Times.Once);
            _teamsRepositoryMock.Verify(x => x.DeleteAsync(teamId2), Times.Once);
        }

        #endregion
    }
}
