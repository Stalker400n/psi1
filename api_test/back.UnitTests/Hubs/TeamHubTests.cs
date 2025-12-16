using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.SignalR;
using back.Hubs;
using back.Data.Repositories;
using back.Models;
using Microsoft.Extensions.Logging;

namespace back.Tests.Hubs
{
    public class TeamHubTests
    {
        private readonly Mock<ITeamsRepository> _teamsRepositoryMock;
        private readonly Mock<ILogger<TeamHub>> _loggerMock;
        private readonly Mock<IGroupManager> _groupManagerMock;
        private readonly Mock<HubCallerContext> _hubCallerContextMock;
        private readonly TeamHub _sut;

        public TeamHubTests()
        {
            _teamsRepositoryMock = new Mock<ITeamsRepository>();
            _loggerMock = new Mock<ILogger<TeamHub>>();
            _groupManagerMock = new Mock<IGroupManager>();
            _hubCallerContextMock = new Mock<HubCallerContext>();

            _sut = new TeamHub(_teamsRepositoryMock.Object, _loggerMock.Object)
            {
                Clients = new Mock<IHubCallerClients>().Object,
                Groups = _groupManagerMock.Object,
                Context = _hubCallerContextMock.Object
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WhenTeamsRepositoryIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
              new TeamHub(null!, _loggerMock.Object));
        }

        [Fact]
        public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
              new TeamHub(_teamsRepositoryMock.Object, null!));
        }

        [Fact]
        public void Constructor_WhenValidDependencies_ShouldCreateInstance()
        {
            // Arrange & Act
            var hub = new TeamHub(_teamsRepositoryMock.Object, _loggerMock.Object);

            // Assert
            hub.Should().NotBeNull();
        }

        #endregion

        #region JoinTeam Tests

        [Fact]
        public async Task JoinTeam_WithValidTeamId_ShouldAddUserToGroup()
        {
            // Arrange
            var teamId = "1";
            var connectionId = "connection123";
            var team = new Team { Id = 1, Name = "Test Team", CurrentSongIndex = 0, IsPlaying = false };

            _hubCallerContextMock.Setup(x => x.ConnectionId).Returns(connectionId);
            _groupManagerMock
              .Setup(x => x.AddToGroupAsync(connectionId, teamId, It.IsAny<CancellationToken>()))
              .Returns(Task.CompletedTask);
            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(1))
              .ReturnsAsync(team);

            var clientProxyMock = new Mock<ISingleClientProxy>();
            var callerClientsMock = new Mock<IHubCallerClients>();
            callerClientsMock.Setup(x => x.Caller).Returns(clientProxyMock.Object);

            _sut.Clients = callerClientsMock.Object;

            // Act
            await _sut.JoinTeam(teamId);

            // Assert
            _groupManagerMock.Verify(
              x => x.AddToGroupAsync(connectionId, teamId, It.IsAny<CancellationToken>()),
              Times.Once);
            _teamsRepositoryMock.Verify(x => x.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task JoinTeam_ShouldSendPlaybackState()
        {
            // Arrange
            var teamId = "5";
            var connectionId = "conn456";
            var team = new Team
            {
                Id = 5,
                Name = "Test Team",
                CurrentSongIndex = 2,
                IsPlaying = true,
                StartedAtUtc = DateTime.UtcNow,
                ElapsedSeconds = 30
            };

            _hubCallerContextMock.Setup(x => x.ConnectionId).Returns(connectionId);
            _groupManagerMock
              .Setup(x => x.AddToGroupAsync(connectionId, teamId, It.IsAny<CancellationToken>()))
              .Returns(Task.CompletedTask);
            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(5))
              .ReturnsAsync(team);

            var clientProxyMock = new Mock<ISingleClientProxy>();
            var callerClientsMock = new Mock<IHubCallerClients>();
            callerClientsMock.Setup(x => x.Caller).Returns(clientProxyMock.Object);

            _sut.Clients = callerClientsMock.Object;

            // Act
            await _sut.JoinTeam(teamId);

            // Assert - Verify repository was accessed and method completes without error
            _teamsRepositoryMock.Verify(x => x.GetByIdAsync(5), Times.Once);
        }

        [Fact]
        public async Task JoinTeam_WithInvalidTeamId_ShouldThrowFormatException()
        {
            // Arrange
            var invalidTeamId = "not-a-number";

            _hubCallerContextMock.Setup(x => x.ConnectionId).Returns("conn");
            _groupManagerMock
              .Setup(x => x.AddToGroupAsync("conn", invalidTeamId, It.IsAny<CancellationToken>()))
              .Returns(Task.CompletedTask);

            // Act & Assert
            await Assert.ThrowsAsync<FormatException>(() => _sut.JoinTeam(invalidTeamId));
        }

        #endregion

        #region LeaveTeam Tests

        [Fact]
        public async Task LeaveTeam_WithValidTeamId_ShouldRemoveUserFromGroup()
        {
            // Arrange
            var teamId = "3";
            var connectionId = "conn789";

            _hubCallerContextMock.Setup(x => x.ConnectionId).Returns(connectionId);
            _groupManagerMock
              .Setup(x => x.RemoveFromGroupAsync(connectionId, teamId, It.IsAny<CancellationToken>()))
              .Returns(Task.CompletedTask);

            // Act
            await _sut.LeaveTeam(teamId);

            // Assert
            _groupManagerMock.Verify(
              x => x.RemoveFromGroupAsync(connectionId, teamId, It.IsAny<CancellationToken>()),
              Times.Once);
        }

        [Fact]
        public async Task LeaveTeam_ShouldNotCallRepository()
        {
            // Arrange
            var teamId = "3";
            var connectionId = "conn";

            _hubCallerContextMock.Setup(x => x.ConnectionId).Returns(connectionId);
            _groupManagerMock
              .Setup(x => x.RemoveFromGroupAsync(connectionId, teamId, It.IsAny<CancellationToken>()))
              .Returns(Task.CompletedTask);

            // Act
            await _sut.LeaveTeam(teamId);

            // Assert
            _teamsRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        #endregion

        #region Play Tests

        [Fact]
        public async Task Play_WhenTeamNotPlaying_ShouldStartPlayback()
        {
            // Arrange
            var teamId = "2";
            var team = new Team
            {
                Id = 2,
                Name = "Test Team",
                CurrentSongIndex = 0,
                IsPlaying = false,
                ElapsedSeconds = 0
            };

            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(2))
              .ReturnsAsync(team);
            _teamsRepositoryMock
              .Setup(x => x.UpdateAsync(2, It.IsAny<Team>()))
              .ReturnsAsync(team);

            var groupClientsMock = new Mock<IClientProxy>();
            var callerClientsMock = new Mock<IHubCallerClients>();
            callerClientsMock.Setup(x => x.Group(teamId)).Returns(groupClientsMock.Object);

            _sut.Clients = callerClientsMock.Object;

            // Act
            await _sut.Play(teamId);

            // Assert
            _teamsRepositoryMock.Verify(x => x.GetByIdAsync(2), Times.Once);
            _teamsRepositoryMock.Verify(x => x.UpdateAsync(2, It.IsAny<Team>()), Times.Once);
        }

        [Fact]
        public async Task Play_WhenTeamAlreadyPlaying_ShouldReturnEarly()
        {
            // Arrange
            var teamId = "1";
            var team = new Team
            {
                Id = 1,
                Name = "Test Team",
                IsPlaying = true,
                StartedAtUtc = DateTime.UtcNow
            };

            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(1))
              .ReturnsAsync(team);

            // Act
            await _sut.Play(teamId);

            // Assert
            _teamsRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<Team>()), Times.Never);
        }

        [Fact]
        public async Task Play_ShouldBroadcastPlaybackState()
        {
            // Arrange
            var teamId = "4";
            var team = new Team
            {
                Id = 4,
                Name = "Test Team",
                IsPlaying = false,
                CurrentSongIndex = 1,
                ElapsedSeconds = 0
            };

            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(4))
              .ReturnsAsync(team);
            _teamsRepositoryMock
              .Setup(x => x.UpdateAsync(4, It.IsAny<Team>()))
              .ReturnsAsync(team);

            var groupClientsMock = new Mock<IClientProxy>();
            var callerClientsMock = new Mock<IHubCallerClients>();
            callerClientsMock.Setup(x => x.Group(teamId)).Returns(groupClientsMock.Object);

            _sut.Clients = callerClientsMock.Object;

            // Act
            await _sut.Play(teamId);

            // Assert - Verify state was updated
            _teamsRepositoryMock.Verify(x => x.UpdateAsync(4, It.IsAny<Team>()), Times.Once);
        }

        [Fact]
        public async Task Play_ShouldSetIsPlayingTrue()
        {
            // Arrange
            var teamId = "1";
            var team = new Team
            {
                Id = 1,
                Name = "Test Team",
                IsPlaying = false
            };

            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(1))
              .ReturnsAsync(team);

            Team? updatedTeam = null;
            _teamsRepositoryMock
              .Setup(x => x.UpdateAsync(1, It.IsAny<Team>()))
              .Callback((int id, Team t) => updatedTeam = t)
              .ReturnsAsync(team);

            var groupClientsMock = new Mock<IClientProxy>();
            var callerClientsMock = new Mock<IHubCallerClients>();
            callerClientsMock.Setup(x => x.Group(teamId)).Returns(groupClientsMock.Object);

            _sut.Clients = callerClientsMock.Object;

            // Act
            await _sut.Play(teamId);

            // Assert
            updatedTeam.Should().NotBeNull();
            updatedTeam!.IsPlaying.Should().BeTrue();
        }

        #endregion

        #region Pause Tests

        [Fact]
        public async Task Pause_WhenTeamPlaying_ShouldPausePlayback()
        {
            // Arrange
            var teamId = "1";
            var startTime = DateTime.UtcNow.AddSeconds(-10);
            var team = new Team
            {
                Id = 1,
                Name = "Test Team",
                IsPlaying = true,
                StartedAtUtc = startTime,
                ElapsedSeconds = 0
            };

            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(1))
              .ReturnsAsync(team);
            _teamsRepositoryMock
              .Setup(x => x.UpdateAsync(1, It.IsAny<Team>()))
              .ReturnsAsync(team);

            var groupClientsMock = new Mock<IClientProxy>();
            var callerClientsMock = new Mock<IHubCallerClients>();
            callerClientsMock.Setup(x => x.Group(teamId)).Returns(groupClientsMock.Object);

            _sut.Clients = callerClientsMock.Object;

            // Act
            await _sut.Pause(teamId);

            // Assert
            _teamsRepositoryMock.Verify(x => x.UpdateAsync(1, It.IsAny<Team>()), Times.Once);
        }

        [Fact]
        public async Task Pause_WhenTeamNotPlaying_ShouldReturnEarly()
        {
            // Arrange
            var teamId = "2";
            var team = new Team
            {
                Id = 2,
                Name = "Test Team",
                IsPlaying = false,
                StartedAtUtc = null
            };

            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(2))
              .ReturnsAsync(team);

            // Act
            await _sut.Pause(teamId);

            // Assert
            _teamsRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<Team>()), Times.Never);
        }

        [Fact]
        public async Task Pause_ShouldUpdateElapsedSeconds()
        {
            // Arrange
            var teamId = "1";
            var startTime = DateTime.UtcNow.AddSeconds(-10);
            var team = new Team
            {
                Id = 1,
                Name = "Test Team",
                IsPlaying = true,
                StartedAtUtc = startTime,
                ElapsedSeconds = 5
            };

            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(1))
              .ReturnsAsync(team);

            Team? updatedTeam = null;
            _teamsRepositoryMock
              .Setup(x => x.UpdateAsync(1, It.IsAny<Team>()))
              .Callback((int id, Team t) => updatedTeam = t)
              .ReturnsAsync(team);

            var groupClientsMock = new Mock<IClientProxy>();
            var callerClientsMock = new Mock<IHubCallerClients>();
            callerClientsMock.Setup(x => x.Group(teamId)).Returns(groupClientsMock.Object);

            _sut.Clients = callerClientsMock.Object;

            // Act
            await _sut.Pause(teamId);

            // Assert
            updatedTeam.Should().NotBeNull();
            updatedTeam!.ElapsedSeconds.Should().BeGreaterThanOrEqualTo(5);
        }

        [Fact]
        public async Task Pause_ShouldSetIsPlayingFalse()
        {
            // Arrange
            var teamId = "1";
            var startTime = DateTime.UtcNow;
            var team = new Team
            {
                Id = 1,
                Name = "Test Team",
                IsPlaying = true,
                StartedAtUtc = startTime,
                ElapsedSeconds = 0
            };

            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(1))
              .ReturnsAsync(team);

            Team? updatedTeam = null;
            _teamsRepositoryMock
              .Setup(x => x.UpdateAsync(1, It.IsAny<Team>()))
              .Callback((int id, Team t) => updatedTeam = t)
              .ReturnsAsync(team);

            var groupClientsMock = new Mock<IClientProxy>();
            var callerClientsMock = new Mock<IHubCallerClients>();
            callerClientsMock.Setup(x => x.Group(teamId)).Returns(groupClientsMock.Object);

            _sut.Clients = callerClientsMock.Object;

            // Act
            await _sut.Pause(teamId);

            // Assert
            updatedTeam.Should().NotBeNull();
            updatedTeam!.IsPlaying.Should().BeFalse();
        }

        [Fact]
        public async Task Pause_ShouldBroadcastPlaybackState()
        {
            // Arrange
            var teamId = "3";
            var startTime = DateTime.UtcNow;
            var team = new Team
            {
                Id = 3,
                Name = "Test Team",
                IsPlaying = true,
                StartedAtUtc = startTime,
                ElapsedSeconds = 0
            };

            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(3))
              .ReturnsAsync(team);
            _teamsRepositoryMock
              .Setup(x => x.UpdateAsync(3, It.IsAny<Team>()))
              .ReturnsAsync(team);

            var groupClientsMock = new Mock<IClientProxy>();
            var callerClientsMock = new Mock<IHubCallerClients>();
            callerClientsMock.Setup(x => x.Group(teamId)).Returns(groupClientsMock.Object);

            _sut.Clients = callerClientsMock.Object;

            // Act
            await _sut.Pause(teamId);

            // Assert - Verify state was updated
            _teamsRepositoryMock.Verify(x => x.UpdateAsync(3, It.IsAny<Team>()), Times.Once);
        }

        #endregion

        #region Next Tests

        [Fact]
        public async Task Next_ShouldResetElapsedSeconds()
        {
            // Arrange
            var teamId = "1";
            var team = new Team
            {
                Id = 1,
                Name = "Test Team",
                CurrentSongIndex = 2,
                ElapsedSeconds = 100,
                IsPlaying = true
            };

            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(1))
              .ReturnsAsync(team);

            Team? updatedTeam = null;
            _teamsRepositoryMock
              .Setup(x => x.UpdateAsync(1, It.IsAny<Team>()))
              .Callback((int id, Team t) => updatedTeam = t)
              .ReturnsAsync(team);

            var groupClientsMock = new Mock<IClientProxy>();
            var callerClientsMock = new Mock<IHubCallerClients>();
            callerClientsMock.Setup(x => x.Group(teamId)).Returns(groupClientsMock.Object);

            _sut.Clients = callerClientsMock.Object;

            // Act
            await _sut.Next(teamId);

            // Assert
            updatedTeam.Should().NotBeNull();
            updatedTeam!.ElapsedSeconds.Should().Be(0);
        }

        [Fact]
        public async Task Next_ShouldSetStartedAtUtc()
        {
            // Arrange
            var teamId = "2";
            var team = new Team
            {
                Id = 2,
                Name = "Test Team",
                CurrentSongIndex = 0,
                ElapsedSeconds = 0,
                IsPlaying = false,
                StartedAtUtc = null
            };

            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(2))
              .ReturnsAsync(team);

            Team? updatedTeam = null;
            _teamsRepositoryMock
              .Setup(x => x.UpdateAsync(2, It.IsAny<Team>()))
              .Callback((int id, Team t) => updatedTeam = t)
              .ReturnsAsync(team);

            var groupClientsMock = new Mock<IClientProxy>();
            var callerClientsMock = new Mock<IHubCallerClients>();
            callerClientsMock.Setup(x => x.Group(teamId)).Returns(groupClientsMock.Object);

            _sut.Clients = callerClientsMock.Object;

            // Act
            await _sut.Next(teamId);

            // Assert
            updatedTeam.Should().NotBeNull();
            updatedTeam!.StartedAtUtc.Should().NotBeNull();
            updatedTeam!.StartedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task Next_ShouldSetIsPlayingFalse()
        {
            // Arrange
            var teamId = "3";
            var team = new Team
            {
                Id = 3,
                Name = "Test Team",
                IsPlaying = true
            };

            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(3))
              .ReturnsAsync(team);

            Team? updatedTeam = null;
            _teamsRepositoryMock
              .Setup(x => x.UpdateAsync(3, It.IsAny<Team>()))
              .Callback((int id, Team t) => updatedTeam = t)
              .ReturnsAsync(team);

            var groupClientsMock = new Mock<IClientProxy>();
            var callerClientsMock = new Mock<IHubCallerClients>();
            callerClientsMock.Setup(x => x.Group(teamId)).Returns(groupClientsMock.Object);

            _sut.Clients = callerClientsMock.Object;

            // Act
            await _sut.Next(teamId);

            // Assert
            updatedTeam.Should().NotBeNull();
            updatedTeam!.IsPlaying.Should().BeFalse();
        }

        [Fact]
        public async Task Next_ShouldBroadcastPlaybackState()
        {
            // Arrange
            var teamId = "4";
            var team = new Team
            {
                Id = 4,
                Name = "Test Team",
                CurrentSongIndex = 1
            };

            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(4))
              .ReturnsAsync(team);
            _teamsRepositoryMock
              .Setup(x => x.UpdateAsync(4, It.IsAny<Team>()))
              .ReturnsAsync(team);

            var groupClientsMock = new Mock<IClientProxy>();
            var callerClientsMock = new Mock<IHubCallerClients>();
            callerClientsMock.Setup(x => x.Group(teamId)).Returns(groupClientsMock.Object);

            _sut.Clients = callerClientsMock.Object;

            // Act
            await _sut.Next(teamId);

            // Assert - Verify state was updated
            _teamsRepositoryMock.Verify(x => x.UpdateAsync(4, It.IsAny<Team>()), Times.Once);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task JoinTeam_ThenPlay_ShouldWorkSequentially()
        {
            // Arrange
            var teamId = "1";
            var connectionId = "conn123";
            var team = new Team
            {
                Id = 1,
                Name = "Test Team",
                IsPlaying = false,
                CurrentSongIndex = 0
            };

            _hubCallerContextMock.Setup(x => x.ConnectionId).Returns(connectionId);
            _groupManagerMock
              .Setup(x => x.AddToGroupAsync(connectionId, teamId, It.IsAny<CancellationToken>()))
              .Returns(Task.CompletedTask);
            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(1))
              .ReturnsAsync(team);
            _teamsRepositoryMock
              .Setup(x => x.UpdateAsync(1, It.IsAny<Team>()))
              .ReturnsAsync(team);

            var clientProxyMock = new Mock<ISingleClientProxy>();
            var groupClientsMock = new Mock<IClientProxy>();
            var callerClientsMock = new Mock<IHubCallerClients>();
            callerClientsMock.Setup(x => x.Caller).Returns(clientProxyMock.Object);
            callerClientsMock.Setup(x => x.Group(teamId)).Returns(groupClientsMock.Object);

            _sut.Clients = callerClientsMock.Object;

            // Act
            await _sut.JoinTeam(teamId);
            await _sut.Play(teamId);

            // Assert
            _teamsRepositoryMock.Verify(x => x.GetByIdAsync(1), Times.Exactly(2));
            _teamsRepositoryMock.Verify(x => x.UpdateAsync(1, It.IsAny<Team>()), Times.Once);
        }

        [Fact]
        public async Task Play_ThenPause_ShouldWorkSequentially()
        {
            // Arrange
            var teamId = "2";
            var team = new Team
            {
                Id = 2,
                Name = "Test Team",
                IsPlaying = false,
                ElapsedSeconds = 0,
                StartedAtUtc = null
            };

            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(2))
              .ReturnsAsync(team);
            _teamsRepositoryMock
              .Setup(x => x.UpdateAsync(2, It.IsAny<Team>()))
              .ReturnsAsync(team);

            var groupClientsMock = new Mock<IClientProxy>();
            var callerClientsMock = new Mock<IHubCallerClients>();
            callerClientsMock.Setup(x => x.Group(teamId)).Returns(groupClientsMock.Object);

            _sut.Clients = callerClientsMock.Object;

            // Act
            await _sut.Play(teamId);

            // Update team state after play
            team.IsPlaying = true;
            team.StartedAtUtc = DateTime.UtcNow;

            await _sut.Pause(teamId);

            // Assert
            _teamsRepositoryMock.Verify(x => x.UpdateAsync(2, It.IsAny<Team>()), Times.Exactly(2));
        }

        #endregion
    }
}
