using Xunit;
using Moq;
using FluentAssertions;
using back.Services;
using back.Models;
using back.Data.Repositories;
using System.Text.Json;

namespace back.Tests.Services
{
  public class DataImportServiceTests : IDisposable
  {
    private readonly Mock<ITeamsRepository> _teamsRepositoryMock;
    private readonly Mock<IUsersRepository> _usersRepositoryMock;
    private readonly Mock<ISongsRepository> _songsRepositoryMock;
    private readonly Mock<IChatsRepository> _chatsRepositoryMock;
    private readonly DataImportService _sut;
    private readonly List<string> _testFiles;

    public DataImportServiceTests()
    {
      _teamsRepositoryMock = new Mock<ITeamsRepository>();
      _usersRepositoryMock = new Mock<IUsersRepository>();
      _songsRepositoryMock = new Mock<ISongsRepository>();
      _chatsRepositoryMock = new Mock<IChatsRepository>();
      _testFiles = new List<string>();

      _sut = new DataImportService(
        _teamsRepositoryMock.Object,
        _usersRepositoryMock.Object,
        _songsRepositoryMock.Object,
        _chatsRepositoryMock.Object
      );
    }

    public void Dispose()
    {
      // Clean up test files
      foreach (var file in _testFiles)
      {
        if (File.Exists(file))
        {
          File.Delete(file);
        }
      }
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WhenTeamsRepositoryIsNull_ShouldThrowArgumentNullException()
    {
      // Act & Assert
      Assert.Throws<ArgumentNullException>(() =>
        new DataImportService(null!, _usersRepositoryMock.Object, _songsRepositoryMock.Object, _chatsRepositoryMock.Object));
    }

    [Fact]
    public void Constructor_WhenUsersRepositoryIsNull_ShouldThrowArgumentNullException()
    {
      // Act & Assert
      Assert.Throws<ArgumentNullException>(() =>
        new DataImportService(_teamsRepositoryMock.Object, null!, _songsRepositoryMock.Object, _chatsRepositoryMock.Object));
    }

    [Fact]
    public void Constructor_WhenSongsRepositoryIsNull_ShouldThrowArgumentNullException()
    {
      // Act & Assert
      Assert.Throws<ArgumentNullException>(() =>
        new DataImportService(_teamsRepositoryMock.Object, _usersRepositoryMock.Object, null!, _chatsRepositoryMock.Object));
    }

    [Fact]
    public void Constructor_WhenChatsRepositoryIsNull_ShouldThrowArgumentNullException()
    {
      // Act & Assert
      Assert.Throws<ArgumentNullException>(() =>
        new DataImportService(_teamsRepositoryMock.Object, _usersRepositoryMock.Object, _songsRepositoryMock.Object, null!));
    }

    #endregion

    #region ImportData Tests - File Validation

    [Fact]
    public async Task ImportData_WhenFileDoesNotExist_ShouldThrowFileNotFoundException()
    {
      // Arrange
      var nonExistentPath = "nonexistent_file.json";

      // Act & Assert
      var exception = await Assert.ThrowsAsync<FileNotFoundException>(() =>
        _sut.ImportData(nonExistentPath));

      exception.Message.Should().Contain(nonExistentPath);
    }

    [Fact]
    public async Task ImportData_WhenFileIsEmpty_ShouldThrowInvalidOperationException()
    {
      // Arrange
      var filePath = CreateTestFile("[]");

      // Act & Assert
      var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
        _sut.ImportData(filePath));

      exception.Message.Should().Contain("No data found to import");
    }

    [Fact]
    public async Task ImportData_WhenFileContainsNull_ShouldThrowInvalidOperationException()
    {
      // Arrange
      var filePath = CreateTestFile("null");

      // Act & Assert
      await Assert.ThrowsAsync<InvalidOperationException>(() =>
        _sut.ImportData(filePath));
    }

    #endregion

    #region ImportData Tests - Single Team

    [Fact]
    public async Task ImportData_WithSingleTeamNoRelatedData_ShouldCreateTeam()
    {
      // Arrange
      var team = new Team { Id = 1, Name = "Team 1" };
      var teams = new List<Team> { team };
      var filePath = CreateTestFile(teams);

      _teamsRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Team>()))
        .ReturnsAsync(team);

      // Act
      await _sut.ImportData(filePath);

      // Assert
      _teamsRepositoryMock.Verify(x => x.CreateAsync(It.Is<Team>(t =>
        t.Name == "Team 1" &&
        t.Users!.Count == 0 &&
        t.Songs!.Count == 0 &&
        t.Messages!.Count == 0
      )), Times.Once);

      _usersRepositoryMock.Verify(x => x.CreateUserAsync(It.IsAny<int>(), It.IsAny<User>()), Times.Never);
      _songsRepositoryMock.Verify(x => x.AddSongAsync(It.IsAny<int>(), It.IsAny<Song>()), Times.Never);
      _chatsRepositoryMock.Verify(x => x.AddMessageAsync(It.IsAny<int>(), It.IsAny<ChatMessage>()), Times.Never);
    }

    [Fact]
    public async Task ImportData_WithTeamAndUsers_ShouldCreateTeamAndUsers()
    {
      // Arrange
      var team = new Team
      {
        Id = 1,
        Name = "Team 1",
        Users = new List<User>
        {
          new User { Id = 1, Name = "User 1" },
          new User { Id = 2, Name = "User 2" }
        }
      };
      var teams = new List<Team> { team };
      var filePath = CreateTestFile(teams);

      _teamsRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Team>()))
        .ReturnsAsync(new Team { Id = 100, Name = "Team 1" });

      // Act
      await _sut.ImportData(filePath);

      // Assert
      _teamsRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Team>()), Times.Once);
      _usersRepositoryMock.Verify(x => x.CreateUserAsync(100, It.Is<User>(u => u.Id == 0 && u.Name == "User 1")), Times.Once);
      _usersRepositoryMock.Verify(x => x.CreateUserAsync(100, It.Is<User>(u => u.Id == 0 && u.Name == "User 2")), Times.Once);
    }

    [Fact]
    public async Task ImportData_WithTeamAndSongs_ShouldCreateTeamAndSongs()
    {
      // Arrange
      var team = new Team
      {
        Id = 1,
        Name = "Team 1",
        Songs = new List<Song>
        {
          new Song { Id = 1, Title = "Song 1", Index = 0 },
          new Song { Id = 2, Title = "Song 2", Index = 1 }
        }
      };
      var teams = new List<Team> { team };
      var filePath = CreateTestFile(teams);

      _teamsRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Team>()))
        .ReturnsAsync(new Team { Id = 100, Name = "Team 1" });

      // Act
      await _sut.ImportData(filePath);

      // Assert
      _teamsRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Team>()), Times.Once);
      _songsRepositoryMock.Verify(x => x.AddSongAsync(100, It.Is<Song>(s => s.Id == 0 && s.Title == "Song 1")), Times.Once);
      _songsRepositoryMock.Verify(x => x.AddSongAsync(100, It.Is<Song>(s => s.Id == 0 && s.Title == "Song 2")), Times.Once);
    }

    [Fact]
    public async Task ImportData_WithTeamAndMessages_ShouldCreateTeamAndMessages()
    {
      // Arrange
      var team = new Team
      {
        Id = 1,
        Name = "Team 1",
        Messages = new List<ChatMessage>
        {
          new ChatMessage { Id = 1, Text = "Hello", UserName = "User1" },
          new ChatMessage { Id = 2, Text = "World", UserName = "User2" }
        }
      };
      var teams = new List<Team> { team };
      var filePath = CreateTestFile(teams);

      _teamsRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Team>()))
        .ReturnsAsync(new Team { Id = 100, Name = "Team 1" });

      // Act
      await _sut.ImportData(filePath);

      // Assert
      _teamsRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Team>()), Times.Once);
      _chatsRepositoryMock.Verify(x => x.AddMessageAsync(100, It.Is<ChatMessage>(m => m.Id == 0 && m.Text == "Hello")), Times.Once);
      _chatsRepositoryMock.Verify(x => x.AddMessageAsync(100, It.Is<ChatMessage>(m => m.Id == 0 && m.Text == "World")), Times.Once);
    }

    [Fact]
    public async Task ImportData_WithCompleteTeam_ShouldCreateAllRelatedData()
    {
      // Arrange
      var team = new Team
      {
        Id = 1,
        Name = "Team 1",
        Users = new List<User> { new User { Id = 1, Name = "User 1" } },
        Songs = new List<Song> { new Song { Id = 1, Title = "Song 1" } },
        Messages = new List<ChatMessage> { new ChatMessage { Id = 1, Text = "Hello", UserName = "User1" } }
      };
      var teams = new List<Team> { team };
      var filePath = CreateTestFile(teams);

      _teamsRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Team>()))
        .ReturnsAsync(new Team { Id = 100, Name = "Team 1" });

      // Act
      await _sut.ImportData(filePath);

      // Assert
      _teamsRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Team>()), Times.Once);
      _usersRepositoryMock.Verify(x => x.CreateUserAsync(100, It.IsAny<User>()), Times.Once);
      _songsRepositoryMock.Verify(x => x.AddSongAsync(100, It.IsAny<Song>()), Times.Once);
      _chatsRepositoryMock.Verify(x => x.AddMessageAsync(100, It.IsAny<ChatMessage>()), Times.Once);
    }

    #endregion

    #region ImportData Tests - Multiple Teams

    [Fact]
    public async Task ImportData_WithMultipleTeams_ShouldCreateAllTeams()
    {
      // Arrange
      var teams = new List<Team>
      {
        new Team { Id = 1, Name = "Team 1" },
        new Team { Id = 2, Name = "Team 2" },
        new Team { Id = 3, Name = "Team 3" }
      };
      var filePath = CreateTestFile(teams);

      _teamsRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Team>()))
        .ReturnsAsync((Team t) => new Team { Id = 100, Name = t.Name });

      // Act
      await _sut.ImportData(filePath);

      // Assert
      _teamsRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Team>()), Times.Exactly(3));
    }

    [Fact]
    public async Task ImportData_WithMultipleTeamsAndRelatedData_ShouldCreateAllData()
    {
      // Arrange
      var teams = new List<Team>
      {
        new Team
        {
          Id = 1,
          Name = "Team 1",
          Users = new List<User> { new User { Id = 1, Name = "User 1" } }
        },
        new Team
        {
          Id = 2,
          Name = "Team 2",
          Songs = new List<Song> { new Song { Id = 1, Title = "Song 1" } }
        }
      };
      var filePath = CreateTestFile(teams);

      _teamsRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Team>()))
        .ReturnsAsync((Team t) => new Team { Id = t.Id == 1 ? 100 : 200, Name = t.Name });

      // Act
      await _sut.ImportData(filePath);

      // Assert
      _teamsRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Team>()), Times.Exactly(2));
      _usersRepositoryMock.Verify(x => x.CreateUserAsync(100, It.IsAny<User>()), Times.Once);
      _songsRepositoryMock.Verify(x => x.AddSongAsync(200, It.IsAny<Song>()), Times.Once);
    }

    #endregion

    #region ImportData Tests - ID Reset

    [Fact]
    public async Task ImportData_ShouldResetUserIdsToZero()
    {
      // Arrange
      var team = new Team
      {
        Id = 1,
        Name = "Team 1",
        Users = new List<User>
        {
          new User { Id = 999, Name = "User 1" }
        }
      };
      var teams = new List<Team> { team };
      var filePath = CreateTestFile(teams);

      _teamsRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Team>()))
        .ReturnsAsync(new Team { Id = 100, Name = "Team 1" });

      // Act
      await _sut.ImportData(filePath);

      // Assert
      _usersRepositoryMock.Verify(x => x.CreateUserAsync(100, It.Is<User>(u => u.Id == 0)), Times.Once);
    }

    [Fact]
    public async Task ImportData_ShouldResetSongIdsToZero()
    {
      // Arrange
      var team = new Team
      {
        Id = 1,
        Name = "Team 1",
        Songs = new List<Song>
        {
          new Song { Id = 999, Title = "Song 1" }
        }
      };
      var teams = new List<Team> { team };
      var filePath = CreateTestFile(teams);

      _teamsRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Team>()))
        .ReturnsAsync(new Team { Id = 100, Name = "Team 1" });

      // Act
      await _sut.ImportData(filePath);

      // Assert
      _songsRepositoryMock.Verify(x => x.AddSongAsync(100, It.Is<Song>(s => s.Id == 0)), Times.Once);
    }

    [Fact]
    public async Task ImportData_ShouldResetMessageIdsToZero()
    {
      // Arrange
      var team = new Team
      {
        Id = 1,
        Name = "Team 1",
        Messages = new List<ChatMessage>
        {
          new ChatMessage { Id = 999, Text = "Hello", UserName = "User1" }
        }
      };
      var teams = new List<Team> { team };
      var filePath = CreateTestFile(teams);

      _teamsRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Team>()))
        .ReturnsAsync(new Team { Id = 100, Name = "Team 1" });

      // Act
      await _sut.ImportData(filePath);

      // Assert
      _chatsRepositoryMock.Verify(x => x.AddMessageAsync(100, It.Is<ChatMessage>(m => m.Id == 0)), Times.Once);
    }

    #endregion

    #region ImportData Tests - Null Collections

    [Fact]
    public async Task ImportData_WithNullUsers_ShouldNotCreateUsers()
    {
      // Arrange
      var team = new Team
      {
        Id = 1,
        Name = "Team 1",
        Users = null!
      };
      var teams = new List<Team> { team };
      var filePath = CreateTestFile(teams);

      _teamsRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Team>()))
        .ReturnsAsync(new Team { Id = 100, Name = "Team 1" });

      // Act
      await _sut.ImportData(filePath);

      // Assert
      _usersRepositoryMock.Verify(x => x.CreateUserAsync(It.IsAny<int>(), It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task ImportData_WithNullSongs_ShouldNotCreateSongs()
    {
      // Arrange
      var team = new Team
      {
        Id = 1,
        Name = "Team 1",
        Songs = null!
      };
      var teams = new List<Team> { team };
      var filePath = CreateTestFile(teams);

      _teamsRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Team>()))
        .ReturnsAsync(new Team { Id = 100, Name = "Team 1" });

      // Act
      await _sut.ImportData(filePath);

      // Assert
      _songsRepositoryMock.Verify(x => x.AddSongAsync(It.IsAny<int>(), It.IsAny<Song>()), Times.Never);
    }

    [Fact]
    public async Task ImportData_WithNullMessages_ShouldNotCreateMessages()
    {
      // Arrange
      var team = new Team
      {
        Id = 1,
        Name = "Team 1",
        Messages = null!
      };
      var teams = new List<Team> { team };
      var filePath = CreateTestFile(teams);

      _teamsRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Team>()))
        .ReturnsAsync(new Team { Id = 100, Name = "Team 1" });

      // Act
      await _sut.ImportData(filePath);

      // Assert
      _chatsRepositoryMock.Verify(x => x.AddMessageAsync(It.IsAny<int>(), It.IsAny<ChatMessage>()), Times.Never);
    }

    #endregion

    #region Helper Methods

    private string CreateTestFile(string content)
    {
      var filePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");
      File.WriteAllText(filePath, content);
      _testFiles.Add(filePath);
      return filePath;
    }

    private string CreateTestFile(List<Team> teams)
    {
      var json = JsonSerializer.Serialize(teams, new JsonSerializerOptions
      {
        WriteIndented = true
      });
      return CreateTestFile(json);
    }

    #endregion
  }
}