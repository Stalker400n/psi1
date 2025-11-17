using Xunit;
using Moq;
using FluentAssertions;
using back.Services;
using back.Models;
using back.Data.Repositories;
using back.Utils;

namespace back.Tests.Services
{
  public class SongQueueServiceTests
  {
    private readonly Mock<ITeamsRepository> _teamsRepositoryMock;
    private readonly Mock<ISongsRepository> _songsRepositoryMock;
    private readonly Mock<IComparableUtils> _comparableUtilsMock;
    private readonly SongQueueService _sut;

    public SongQueueServiceTests()
    {
      _teamsRepositoryMock = new Mock<ITeamsRepository>();
      _songsRepositoryMock = new Mock<ISongsRepository>();
      _comparableUtilsMock = new Mock<IComparableUtils>();
      _sut = new SongQueueService(
        _teamsRepositoryMock.Object,
        _songsRepositoryMock.Object,
        _comparableUtilsMock.Object
      );
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WhenTeamsRepositoryIsNull_ShouldThrowArgumentNullException()
    {
      // Act & Assert
      Assert.Throws<ArgumentNullException>(() => 
        new SongQueueService(null!, _songsRepositoryMock.Object, _comparableUtilsMock.Object));
    }

    [Fact]
    public void Constructor_WhenSongsRepositoryIsNull_ShouldThrowArgumentNullException()
    {
      // Act & Assert
      Assert.Throws<ArgumentNullException>(() => 
        new SongQueueService(_teamsRepositoryMock.Object, null!, _comparableUtilsMock.Object));
    }

    [Fact]
    public void Constructor_WhenComparableUtilsIsNull_ShouldThrowArgumentNullException()
    {
      // Act & Assert
      Assert.Throws<ArgumentNullException>(() => 
        new SongQueueService(_teamsRepositoryMock.Object, _songsRepositoryMock.Object, null!));
    }

    #endregion


    #region InitializeQueueAsync Tests

    [Fact]
    public async Task InitializeQueueAsync_WhenTeamDoesNotExist_ShouldReturnEarly()
    {
      // Arrange
      var teamId = 1;
      _teamsRepositoryMock.Setup(x => x.GetByIdAsync(teamId))
        .ReturnsAsync((Team?)null);

      // Act
      await _sut.InitializeQueueAsync(teamId);

      // Assert
      _songsRepositoryMock.Verify(x => x.GetSongsAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task InitializeQueueAsync_WhenSongsAreNull_ShouldReturnEarly()
    {
      // Arrange
      var teamId = 1;
      var team = new Team { Id = teamId, CurrentSongIndex = 0 };
      _teamsRepositoryMock.Setup(x => x.GetByIdAsync(teamId)).ReturnsAsync(team);
      _songsRepositoryMock.Setup(x => x.GetSongsAsync(teamId))
        .ReturnsAsync((IEnumerable<Song>?)null);

      // Act
      await _sut.InitializeQueueAsync(teamId);

      // Assert
      _teamsRepositoryMock.Verify(x => x.GetByIdAsync(teamId), Times.Once);
      _songsRepositoryMock.Verify(x => x.GetSongsAsync(teamId), Times.Once);
    }

    [Fact]
    public async Task InitializeQueueAsync_ShouldEnqueueSongsFromCurrentIndexOnwards()
    {
      // Arrange
      var teamId = 1;
      var team = new Team { Id = teamId, CurrentSongIndex = 2 };
      var songs = new List<Song>
      {
        new Song { Id = 1, Index = 0, Title = "Song 0" },
        new Song { Id = 2, Index = 1, Title = "Song 1" },
        new Song { Id = 3, Index = 2, Title = "Song 2" },
        new Song { Id = 4, Index = 3, Title = "Song 3" },
        new Song { Id = 5, Index = 4, Title = "Song 4" }
      };

      _teamsRepositoryMock.Setup(x => x.GetByIdAsync(teamId)).ReturnsAsync(team);
      _songsRepositoryMock.Setup(x => x.GetSongsAsync(teamId)).ReturnsAsync(songs);

      // Act
      await _sut.InitializeQueueAsync(teamId);
      var queue = await _sut.GetQueueAsync(teamId);

      // Assert
      queue.Should().HaveCount(3);
      queue[0].Index.Should().Be(2);
      queue[1].Index.Should().Be(3);
      queue[2].Index.Should().Be(4);
    }

    #endregion

    #region GetQueueAsync Tests

    [Fact]
    public async Task GetQueueAsync_WhenQueueIsEmpty_ShouldInitializeQueue()
    {
      // Arrange
      var teamId = 1;
      var team = new Team { Id = teamId, CurrentSongIndex = 0 };
      var songs = new List<Song>
      {
        new Song { Id = 1, Index = 0, Title = "Song 0" }
      };

      _teamsRepositoryMock.Setup(x => x.GetByIdAsync(teamId)).ReturnsAsync(team);
      _songsRepositoryMock.Setup(x => x.GetSongsAsync(teamId)).ReturnsAsync(songs);

      // Act
      var result = await _sut.GetQueueAsync(teamId);

      // Assert
      result.Should().NotBeEmpty();
      _teamsRepositoryMock.Verify(x => x.GetByIdAsync(teamId), Times.Once);
    }

    [Fact]
    public async Task GetQueueAsync_WhenQueueIsPopulated_ShouldReturnExistingQueue()
    {
      // Arrange
      var teamId = 1;
      var team = new Team { Id = teamId, CurrentSongIndex = 0 };
      var songs = new List<Song>
      {
        new Song { Id = 1, Index = 0, Title = "Song 0" }
      };

      _teamsRepositoryMock.Setup(x => x.GetByIdAsync(teamId)).ReturnsAsync(team);
      _songsRepositoryMock.Setup(x => x.GetSongsAsync(teamId)).ReturnsAsync(songs);

      await _sut.InitializeQueueAsync(teamId);

      // Act
      var result = await _sut.GetQueueAsync(teamId);

      // Assert
      result.Should().HaveCount(1);
    }

    #endregion

    #region GetCurrentSongAsync Tests

    [Fact]
    public async Task GetCurrentSongAsync_WhenTeamDoesNotExist_ShouldReturnNull()
    {
      // Arrange
      var teamId = 1;
      _teamsRepositoryMock.Setup(x => x.GetByIdAsync(teamId))
        .ReturnsAsync((Team?)null);

      // Act
      var result = await _sut.GetCurrentSongAsync(teamId);

      // Assert
      result.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentSongAsync_WhenSongsAreNull_ShouldReturnNull()
    {
      // Arrange
      var teamId = 1;
      var team = new Team { Id = teamId, CurrentSongIndex = 0 };
      _teamsRepositoryMock.Setup(x => x.GetByIdAsync(teamId)).ReturnsAsync(team);
      _songsRepositoryMock.Setup(x => x.GetSongsAsync(teamId))
        .ReturnsAsync((IEnumerable<Song>?)null);

      // Act
      var result = await _sut.GetCurrentSongAsync(teamId);

      // Assert
      result.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentSongAsync_ShouldReturnSongAtCurrentIndex()
    {
      // Arrange
      var teamId = 1;
      var team = new Team { Id = teamId, CurrentSongIndex = 2 };
      var songs = new List<Song>
      {
        new Song { Id = 1, Index = 0, Title = "Song 0" },
        new Song { Id = 2, Index = 1, Title = "Song 1" },
        new Song { Id = 3, Index = 2, Title = "Song 2" }
      };

      _teamsRepositoryMock.Setup(x => x.GetByIdAsync(teamId)).ReturnsAsync(team);
      _songsRepositoryMock.Setup(x => x.GetSongsAsync(teamId)).ReturnsAsync(songs);

      // Act
      var result = await _sut.GetCurrentSongAsync(teamId);

      // Assert
      result.Should().NotBeNull();
      result!.Index.Should().Be(2);
      result.Title.Should().Be("Song 2");
    }

    #endregion

    #region AdvanceToNextSongAsync Tests

    [Fact]
    public async Task AdvanceToNextSongAsync_WhenTeamDoesNotExist_ShouldReturnNull()
    {
      // Arrange
      var teamId = 1;
      _teamsRepositoryMock.Setup(x => x.GetByIdAsync(teamId))
        .ReturnsAsync((Team?)null);

      // Act
      var result = await _sut.AdvanceToNextSongAsync(teamId);

      // Assert
      result.Should().BeNull();
    }

    [Fact]
    public async Task AdvanceToNextSongAsync_WhenSongsAreNull_ShouldReturnNull()
    {
      // Arrange
      var teamId = 1;
      var team = new Team { Id = teamId, CurrentSongIndex = 0 };
      _teamsRepositoryMock.Setup(x => x.GetByIdAsync(teamId)).ReturnsAsync(team);
      _songsRepositoryMock.Setup(x => x.GetSongsAsync(teamId))
        .ReturnsAsync((IEnumerable<Song>?)null);

      // Act
      var result = await _sut.AdvanceToNextSongAsync(teamId);

      // Assert
      result.Should().BeNull();
    }

    [Fact]
    public async Task AdvanceToNextSongAsync_WhenAtLastSong_ShouldReturnNull()
    {
      // Arrange
      var teamId = 1;
      var team = new Team { Id = teamId, CurrentSongIndex = 2 };
      var songs = new List<Song>
      {
        new Song { Id = 1, Index = 0, Title = "Song 0" },
        new Song { Id = 2, Index = 1, Title = "Song 1" },
        new Song { Id = 3, Index = 2, Title = "Song 2" }
      };

      _teamsRepositoryMock.Setup(x => x.GetByIdAsync(teamId)).ReturnsAsync(team);
      _songsRepositoryMock.Setup(x => x.GetSongsAsync(teamId)).ReturnsAsync(songs);

      // Act
      var result = await _sut.AdvanceToNextSongAsync(teamId);

      // Assert
      result.Should().BeNull();
    }

    [Fact]
    public async Task AdvanceToNextSongAsync_ShouldIncrementIndexAndReturnNextSong()
    {
      // Arrange
      var teamId = 1;
      var team = new Team { Id = teamId, CurrentSongIndex = 0 };
      var songs = new List<Song>
      {
        new Song { Id = 1, Index = 0, Title = "Song 0" },
        new Song { Id = 2, Index = 1, Title = "Song 1" },
        new Song { Id = 3, Index = 2, Title = "Song 2" }
      };

      _teamsRepositoryMock.Setup(x => x.GetByIdAsync(teamId)).ReturnsAsync(team);
      _songsRepositoryMock.Setup(x => x.GetSongsAsync(teamId)).ReturnsAsync(songs);
      // Act
      var result = await _sut.AdvanceToNextSongAsync(teamId);

      // Assert
      result.Should().NotBeNull();
      result!.Index.Should().Be(1);
      result.Title.Should().Be("Song 1");
      team.CurrentSongIndex.Should().Be(1);
      _teamsRepositoryMock.Verify(x => x.UpdateAsync(teamId, team), Times.Once);
    }

    #endregion

    #region GoToPreviousSongAsync Tests

    [Fact]
    public async Task GoToPreviousSongAsync_WhenTeamDoesNotExist_ShouldReturnNull()
    {
      // Arrange
      var teamId = 1;
      _teamsRepositoryMock.Setup(x => x.GetByIdAsync(teamId))
        .ReturnsAsync((Team?)null);

      // Act
      var result = await _sut.GoToPreviousSongAsync(teamId);

      // Assert
      result.Should().BeNull();
    }

    [Fact]
    public async Task GoToPreviousSongAsync_WhenAtFirstSong_ShouldReturnNull()
    {
      // Arrange
      var teamId = 1;
      var team = new Team { Id = teamId, CurrentSongIndex = 0 };
      _teamsRepositoryMock.Setup(x => x.GetByIdAsync(teamId)).ReturnsAsync(team);

      // Act
      var result = await _sut.GoToPreviousSongAsync(teamId);

      // Assert
      result.Should().BeNull();
    }

    [Fact]
    public async Task GoToPreviousSongAsync_ShouldDecrementIndexAndReturnPreviousSong()
    {
      // Arrange
      var teamId = 1;
      var team = new Team { Id = teamId, CurrentSongIndex = 2 };
      var songs = new List<Song>
      {
        new Song { Id = 1, Index = 0, Title = "Song 0" },
        new Song { Id = 2, Index = 1, Title = "Song 1" },
        new Song { Id = 3, Index = 2, Title = "Song 2" }
      };

      _teamsRepositoryMock.Setup(x => x.GetByIdAsync(teamId)).ReturnsAsync(team);
      _songsRepositoryMock.Setup(x => x.GetSongsAsync(teamId)).ReturnsAsync(songs);
      // Act
      var result = await _sut.GoToPreviousSongAsync(teamId);

      // Assert
      result.Should().NotBeNull();
      result!.Index.Should().Be(1);
      result.Title.Should().Be("Song 1");
      team.CurrentSongIndex.Should().Be(1);
      _teamsRepositoryMock.Verify(x => x.UpdateAsync(teamId, team), Times.Once);
    }

    #endregion

    #region JumpToSongAsync Tests

    [Fact]
    public async Task JumpToSongAsync_WhenTeamDoesNotExist_ShouldReturnNull()
    {
      // Arrange
      var teamId = 1;
      var index = 2;
      _teamsRepositoryMock.Setup(x => x.GetByIdAsync(teamId))
        .ReturnsAsync((Team?)null);

      // Act
      var result = await _sut.JumpToSongAsync(teamId, index);

      // Assert
      result.Should().BeNull();
    }

    [Fact]
    public async Task JumpToSongAsync_WhenSongsAreNull_ShouldReturnNull()
    {
      // Arrange
      var teamId = 1;
      var index = 2;
      var team = new Team { Id = teamId, CurrentSongIndex = 0 };
      _teamsRepositoryMock.Setup(x => x.GetByIdAsync(teamId)).ReturnsAsync(team);
      _songsRepositoryMock.Setup(x => x.GetSongsAsync(teamId))
        .ReturnsAsync((IEnumerable<Song>?)null);

      // Act
      var result = await _sut.JumpToSongAsync(teamId, index);

      // Assert
      result.Should().BeNull();
    }

    [Fact]
    public async Task JumpToSongAsync_WhenSongDoesNotExist_ShouldReturnNull()
    {
      // Arrange
      var teamId = 1;
      var index = 5;
      var team = new Team { Id = teamId, CurrentSongIndex = 0 };
      var songs = new List<Song>
      {
        new Song { Id = 1, Index = 0, Title = "Song 0" },
        new Song { Id = 2, Index = 1, Title = "Song 1" }
      };

      _teamsRepositoryMock.Setup(x => x.GetByIdAsync(teamId)).ReturnsAsync(team);
      _songsRepositoryMock.Setup(x => x.GetSongsAsync(teamId)).ReturnsAsync(songs);

      // Act
      var result = await _sut.JumpToSongAsync(teamId, index);

      // Assert
      result.Should().BeNull();
    }

    [Fact]
    public async Task JumpToSongAsync_ShouldUpdateIndexAndReturnTargetSong()
    {
      // Arrange
      var teamId = 1;
      var index = 2;
      var team = new Team { Id = teamId, CurrentSongIndex = 0 };
      var songs = new List<Song>
      {
        new Song { Id = 1, Index = 0, Title = "Song 0" },
        new Song { Id = 2, Index = 1, Title = "Song 1" },
        new Song { Id = 3, Index = 2, Title = "Song 2" }
      };

      _teamsRepositoryMock.Setup(x => x.GetByIdAsync(teamId)).ReturnsAsync(team);
      _songsRepositoryMock.Setup(x => x.GetSongsAsync(teamId)).ReturnsAsync(songs);
      // Act
      var result = await _sut.JumpToSongAsync(teamId, index);

      // Assert
      result.Should().NotBeNull();
      result!.Index.Should().Be(2);
      result.Title.Should().Be("Song 2");
      team.CurrentSongIndex.Should().Be(2);
      _teamsRepositoryMock.Verify(x => x.UpdateAsync(teamId, team), Times.Once);
    }

    #endregion

    #region RefreshQueueAsync Tests

    [Fact]
    public async Task RefreshQueueAsync_ShouldCallInitializeQueue()
    {
      // Arrange
      var teamId = 1;
      var team = new Team { Id = teamId, CurrentSongIndex = 0 };
      var songs = new List<Song>
      {
        new Song { Id = 1, Index = 0, Title = "Song 0" }
      };

      _teamsRepositoryMock.Setup(x => x.GetByIdAsync(teamId)).ReturnsAsync(team);
      _songsRepositoryMock.Setup(x => x.GetSongsAsync(teamId)).ReturnsAsync(songs);

      // Act
      await _sut.RefreshQueueAsync(teamId);

      // Assert
      _teamsRepositoryMock.Verify(x => x.GetByIdAsync(teamId), Times.Once);
      _songsRepositoryMock.Verify(x => x.GetSongsAsync(teamId), Times.Once);
    }

    #endregion

    #region GetSongsSortedByRatingAsync Tests

    [Fact]
    public async Task GetSongsSortedByRatingAsync_ShouldReturnSortedSongs()
    {
      // Arrange
      var teamId = 1;
      var team = new Team { Id = teamId, CurrentSongIndex = 0 };
      var songs = new List<Song>
      {
        new Song { Id = 1, Index = 0, Title = "Song 0" },
        new Song { Id = 2, Index = 1, Title = "Song 1" }
      };
      var sortedSongs = songs.OrderByDescending(s => s.Id).ToList();

      _teamsRepositoryMock.Setup(x => x.GetByIdAsync(teamId)).ReturnsAsync(team);
      _songsRepositoryMock.Setup(x => x.GetSongsAsync(teamId)).ReturnsAsync(songs);
      _comparableUtilsMock.Setup(x => x.SortByComparable(It.IsAny<List<Song>>()))
        .Returns(sortedSongs);

      // Act
      var result = await _sut.GetSongsSortedByRatingAsync(teamId);

      // Assert
      result.Should().Equal(sortedSongs);
      _comparableUtilsMock.Verify(x => x.SortByComparable(It.IsAny<List<Song>>()), Times.Once);
    }

    #endregion

    #region GetLowestRatedSongAsync Tests

    [Fact]
    public async Task GetLowestRatedSongAsync_ShouldReturnLowestRatedSong()
    {
      // Arrange
      var teamId = 1;
      var team = new Team { Id = teamId, CurrentSongIndex = 0 };
      var songs = new List<Song>
      {
        new Song { Id = 1, Index = 0, Title = "Song 0" },
        new Song { Id = 2, Index = 1, Title = "Song 1" }
      };
      var lowestSong = songs.First();

      _teamsRepositoryMock.Setup(x => x.GetByIdAsync(teamId)).ReturnsAsync(team);
      _songsRepositoryMock.Setup(x => x.GetSongsAsync(teamId)).ReturnsAsync(songs);
      _comparableUtilsMock.Setup(x => x.FindMinimum(It.IsAny<List<Song>>()))
        .Returns(lowestSong);

      // Act
      var result = await _sut.GetLowestRatedSongAsync(teamId);

      // Assert
      result.Should().Be(lowestSong);
      _comparableUtilsMock.Verify(x => x.FindMinimum(It.IsAny<List<Song>>()), Times.Once);
    }

    [Fact]
    public async Task GetLowestRatedSongAsync_WhenNoSongs_ShouldReturnNull()
    {
      // Arrange
      var teamId = 1;
      var team = new Team { Id = teamId, CurrentSongIndex = 0 };
      var songs = new List<Song>();

      _teamsRepositoryMock.Setup(x => x.GetByIdAsync(teamId)).ReturnsAsync(team);
      _songsRepositoryMock.Setup(x => x.GetSongsAsync(teamId)).ReturnsAsync(songs);
      _comparableUtilsMock.Setup(x => x.FindMinimum(It.IsAny<List<Song>>()))
        .Returns((Song?)null);

      // Act
      var result = await _sut.GetLowestRatedSongAsync(teamId);

      // Assert
      result.Should().BeNull();
    }

    #endregion
  }
}