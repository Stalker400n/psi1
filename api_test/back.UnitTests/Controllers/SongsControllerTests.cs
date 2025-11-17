using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using back.Controllers;
using back.Models;
using back.Data.Repositories;
using back.Services;
using back.Validators;
using back.Exceptions;
using Microsoft.Extensions.Logging;

namespace back.Tests.Controllers
{
    public class SongsControllerTests
    {
        private readonly Mock<ISongsRepository> _songsRepositoryMock;
        private readonly Mock<ITeamsRepository> _teamsRepositoryMock;
        private readonly Mock<ISongQueueService> _queueServiceMock;
        private readonly Mock<IYoutubeValidator> _youtubeValidatorMock;
        private readonly Mock<ILogger<SongsController>> _loggerMock;
        private readonly SongsController _sut;

        public SongsControllerTests()
        {
            _songsRepositoryMock = new Mock<ISongsRepository>();
            _teamsRepositoryMock = new Mock<ITeamsRepository>();
            _queueServiceMock = new Mock<ISongQueueService>();
            _youtubeValidatorMock = new Mock<IYoutubeValidator>();
            _loggerMock = new Mock<ILogger<SongsController>>();

            _sut = new SongsController(
              _songsRepositoryMock.Object,
              _teamsRepositoryMock.Object,
              _queueServiceMock.Object,
              _youtubeValidatorMock.Object,
              _loggerMock.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WhenSongsRepositoryIsNull_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
              new SongsController(
                null!,
                _teamsRepositoryMock.Object,
                _queueServiceMock.Object,
                _youtubeValidatorMock.Object,
                _loggerMock.Object));
        }

        [Fact]
        public void Constructor_WhenTeamsRepositoryIsNull_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
              new SongsController(
                _songsRepositoryMock.Object,
                null!,
                _queueServiceMock.Object,
                _youtubeValidatorMock.Object,
                _loggerMock.Object));
        }

        [Fact]
        public void Constructor_WhenQueueServiceIsNull_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
              new SongsController(
                _songsRepositoryMock.Object,
                _teamsRepositoryMock.Object,
                null!,
                _youtubeValidatorMock.Object,
                _loggerMock.Object));
        }

        [Fact]
        public void Constructor_WhenYoutubeValidatorIsNull_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
              new SongsController(
                _songsRepositoryMock.Object,
                _teamsRepositoryMock.Object,
                _queueServiceMock.Object,
                null!,
                _loggerMock.Object));
        }

        [Fact]
        public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
              new SongsController(
                _songsRepositoryMock.Object,
                _teamsRepositoryMock.Object,
                _queueServiceMock.Object,
                _youtubeValidatorMock.Object,
                null!));
        }

        #endregion

        #region GetSongs Tests

        [Fact]
        public async Task GetSongs_WithValidTeamId_ShouldReturnOkWithSongs()
        {
            // Arrange
            int teamId = 1;
            var songs = new List<Song>
      {
        new Song { Id = 1, Title = "Song 1", Link = "https://youtube.com/watch?v=test1", Index = 0 },
        new Song { Id = 2, Title = "Song 2", Link = "https://youtube.com/watch?v=test2", Index = 1 }
      };
            _songsRepositoryMock
              .Setup(x => x.GetSongsAsync(teamId))
              .ReturnsAsync(songs);

            // Act
            var result = await _sut.GetSongs(teamId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.StatusCode.Should().Be(200);
            var returnedSongs = Assert.IsAssignableFrom<IEnumerable<Song>>(okResult.Value);
            returnedSongs.Should().HaveCount(2);
            _songsRepositoryMock.Verify(x => x.GetSongsAsync(teamId), Times.Once);
        }

        [Fact]
        public async Task GetSongs_WhenTeamNotFound_ShouldReturnNotFound()
        {
            // Arrange
            int teamId = 999;
            _songsRepositoryMock
              .Setup(x => x.GetSongsAsync(teamId))
              .ReturnsAsync((IEnumerable<Song>?)null);

            // Act
            var result = await _sut.GetSongs(teamId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            notFoundResult.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task GetSongs_WithEmptyList_ShouldReturnOkWithEmptySongs()
        {
            // Arrange
            int teamId = 1;
            var emptySongs = new List<Song>();
            _songsRepositoryMock
              .Setup(x => x.GetSongsAsync(teamId))
              .ReturnsAsync(emptySongs);

            // Act
            var result = await _sut.GetSongs(teamId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedSongs = Assert.IsAssignableFrom<IEnumerable<Song>>(okResult.Value);
            returnedSongs.Should().BeEmpty();
        }

        #endregion

        #region GetSong Tests

        [Fact]
        public async Task GetSong_WithValidTeamIdAndSongId_ShouldReturnOkWithSong()
        {
            // Arrange
            int teamId = 1;
            int songId = 1;
            var song = new Song { Id = 1, Title = "Test Song", Link = "https://youtube.com/watch?v=test", Index = 0 };
            _songsRepositoryMock
              .Setup(x => x.GetSongAsync(teamId, songId))
              .ReturnsAsync(song);

            // Act
            var result = await _sut.GetSong(teamId, songId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.StatusCode.Should().Be(200);
            var returnedSong = Assert.IsType<Song>(okResult.Value);
            returnedSong.Id.Should().Be(1);
            _songsRepositoryMock.Verify(x => x.GetSongAsync(teamId, songId), Times.Once);
        }

        [Fact]
        public async Task GetSong_WhenSongNotFound_ShouldReturnNotFound()
        {
            // Arrange
            int teamId = 1;
            int songId = 999;
            _songsRepositoryMock
              .Setup(x => x.GetSongAsync(teamId, songId))
              .ReturnsAsync((Song?)null);

            // Act
            var result = await _sut.GetSong(teamId, songId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            notFoundResult.StatusCode.Should().Be(404);
        }

        #endregion

        #region AddSong Tests

        [Fact]
        public async Task AddSong_WithValidSongAndTeam_ShouldReturnCreatedAtAction()
        {
            // Arrange
            int teamId = 1;
            var song = new Song { Id = 0, Title = "New Song", Link = "https://youtube.com/watch?v=newtest", Index = 0 };
            var createdSong = song with { Id = 3 };
            var team = new Team { Id = teamId, Name = "Test Team", CurrentSongIndex = 0, Songs = new List<Song>() };

            _youtubeValidatorMock
              .Setup(x => x.ValidateLink(song.Link))
              .Returns(Task.CompletedTask);
            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(teamId))
              .ReturnsAsync(team);
            _songsRepositoryMock
              .Setup(x => x.AddSongAsync(teamId, It.IsAny<Song>()))
              .ReturnsAsync(createdSong);
            _queueServiceMock
              .Setup(x => x.RefreshQueueAsync(teamId))
              .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.AddSong(teamId, song, insertAfterCurrent: false);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            createdResult.StatusCode.Should().Be(201);
            createdResult.ActionName.Should().Be(nameof(SongsController.GetSong));
            var returnedSong = Assert.IsType<Song>(createdResult.Value);
            returnedSong.Id.Should().Be(3);
            _youtubeValidatorMock.Verify(x => x.ValidateLink(song.Link), Times.Once);
            _queueServiceMock.Verify(x => x.RefreshQueueAsync(teamId), Times.Once);
        }

        [Fact]
        public async Task AddSong_WhenYoutubeValidationFails_ShouldReturnBadRequest()
        {
            // Arrange
            int teamId = 1;
            var song = new Song { Id = 0, Title = "Invalid Song", Link = "https://invalid.com", Index = 0 };
            var validationException = new YoutubeValidationException("Invalid YouTube link");

            _youtubeValidatorMock
              .Setup(x => x.ValidateLink(song.Link))
              .ThrowsAsync(validationException);

            // Act
            var result = await _sut.AddSong(teamId, song, insertAfterCurrent: false);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            badRequestResult.StatusCode.Should().Be(400);
            _youtubeValidatorMock.Verify(x => x.ValidateLink(song.Link), Times.Once);
            _songsRepositoryMock.Verify(x => x.AddSongAsync(It.IsAny<int>(), It.IsAny<Song>()), Times.Never);
        }

        [Fact]
        public async Task AddSong_WhenTeamNotFound_ShouldReturnNotFound()
        {
            // Arrange
            int teamId = 999;
            var song = new Song { Id = 0, Title = "New Song", Link = "https://youtube.com/watch?v=test", Index = 0 };

            _youtubeValidatorMock
              .Setup(x => x.ValidateLink(song.Link))
              .Returns(Task.CompletedTask);
            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(teamId))
              .ReturnsAsync((Team?)null);

            // Act
            var result = await _sut.AddSong(teamId, song, insertAfterCurrent: false);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            notFoundResult.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task AddSong_WithInsertAfterCurrentTrue_ShouldInsertAfterCurrentSong()
        {
            // Arrange
            int teamId = 1;
            var existingSong1 = new Song { Id = 1, Title = "Song 1", Link = "https://youtube.com/watch?v=test1", Index = 0 };
            var existingSong2 = new Song { Id = 2, Title = "Song 2", Link = "https://youtube.com/watch?v=test2", Index = 1 };
            var newSong = new Song { Id = 0, Title = "New Song", Link = "https://youtube.com/watch?v=newtest", Index = 0 };
            var team = new Team
            {
                Id = teamId,
                Name = "Test Team",
                CurrentSongIndex = 0,
                Songs = new List<Song> { existingSong1, existingSong2 }
            };
            var createdSong = newSong with { Id = 3, Index = 1 };

            _youtubeValidatorMock
              .Setup(x => x.ValidateLink(newSong.Link))
              .Returns(Task.CompletedTask);
            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(teamId))
              .ReturnsAsync(team);
            _songsRepositoryMock
              .Setup(x => x.UpdateSongAsync(teamId, It.IsAny<int>(), It.IsAny<Song>()))
              .ReturnsAsync((int tid, int sid, Song s) => s);
            _songsRepositoryMock
              .Setup(x => x.AddSongAsync(teamId, It.IsAny<Song>()))
              .ReturnsAsync(createdSong);
            _queueServiceMock
              .Setup(x => x.RefreshQueueAsync(teamId))
              .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.AddSong(teamId, newSong, insertAfterCurrent: true);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            createdResult.StatusCode.Should().Be(201);
            _songsRepositoryMock.Verify(x => x.UpdateSongAsync(teamId, 2, It.IsAny<Song>()), Times.Once);
            _queueServiceMock.Verify(x => x.RefreshQueueAsync(teamId), Times.Once);
        }

        [Fact]
        public async Task AddSong_WithNoExistingSongs_ShouldSetIndexToZero()
        {
            // Arrange
            int teamId = 1;
            var song = new Song { Id = 0, Title = "First Song", Link = "https://youtube.com/watch?v=test", Index = 0 };
            var team = new Team { Id = teamId, Name = "Test Team", CurrentSongIndex = 0, Songs = new List<Song>() };
            var createdSong = song with { Id = 1, Index = 0 };

            _youtubeValidatorMock
              .Setup(x => x.ValidateLink(song.Link))
              .Returns(Task.CompletedTask);
            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(teamId))
              .ReturnsAsync(team);
            _songsRepositoryMock
              .Setup(x => x.AddSongAsync(teamId, It.IsAny<Song>()))
              .ReturnsAsync(createdSong);
            _queueServiceMock
              .Setup(x => x.RefreshQueueAsync(teamId))
              .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.AddSong(teamId, song, insertAfterCurrent: false);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            createdResult.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task AddSong_WhenAddFails_ShouldReturnNotFound()
        {
            // Arrange
            int teamId = 1;
            var song = new Song { Id = 0, Title = "New Song", Link = "https://youtube.com/watch?v=test", Index = 0 };
            var team = new Team { Id = teamId, Name = "Test Team", CurrentSongIndex = 0, Songs = new List<Song>() };

            _youtubeValidatorMock
              .Setup(x => x.ValidateLink(song.Link))
              .Returns(Task.CompletedTask);
            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(teamId))
              .ReturnsAsync(team);
            _songsRepositoryMock
              .Setup(x => x.AddSongAsync(teamId, It.IsAny<Song>()))
              .ReturnsAsync((Song?)null);

            // Act
            var result = await _sut.AddSong(teamId, song, insertAfterCurrent: false);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            notFoundResult.StatusCode.Should().Be(404);
        }

        #endregion

        #region UpdateSong Tests

        [Fact]
        public async Task UpdateSong_WithValidSong_ShouldReturnOkWithUpdatedSong()
        {
            // Arrange
            int teamId = 1;
            int songId = 1;
            var updatedSong = new Song { Id = 1, Title = "Updated Song", Link = "https://youtube.com/watch?v=updated", Index = 0 };

            _songsRepositoryMock
              .Setup(x => x.UpdateSongAsync(teamId, songId, updatedSong))
              .ReturnsAsync(updatedSong);
            _queueServiceMock
              .Setup(x => x.RefreshQueueAsync(teamId))
              .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.UpdateSong(teamId, songId, updatedSong);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.StatusCode.Should().Be(200);
            var returnedSong = Assert.IsType<Song>(okResult.Value);
            returnedSong.Title.Should().Be("Updated Song");
            _queueServiceMock.Verify(x => x.RefreshQueueAsync(teamId), Times.Once);
        }

        [Fact]
        public async Task UpdateSong_WhenSongNotFound_ShouldReturnNotFound()
        {
            // Arrange
            int teamId = 1;
            int songId = 999;
            var song = new Song { Id = 999, Title = "Non-existent", Link = "https://youtube.com/watch?v=test", Index = 0 };

            _songsRepositoryMock
              .Setup(x => x.UpdateSongAsync(teamId, songId, song))
              .ReturnsAsync((Song?)null);

            // Act
            var result = await _sut.UpdateSong(teamId, songId, song);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            notFoundResult.StatusCode.Should().Be(404);
        }

        #endregion

        #region DeleteSong Tests

        [Fact]
        public async Task DeleteSong_WithValidSong_ShouldReturnNoContent()
        {
            // Arrange
            int teamId = 1;
            int songId = 1;
            var songToDelete = new Song { Id = 1, Title = "Song to Delete", Link = "https://youtube.com/watch?v=test", Index = 0 };
            var team = new Team
            {
                Id = teamId,
                Name = "Test Team",
                CurrentSongIndex = 0,
                Songs = new List<Song> { songToDelete }
            };

            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(teamId))
              .ReturnsAsync(team);
            _songsRepositoryMock
              .Setup(x => x.DeleteSongAsync(teamId, songId))
              .ReturnsAsync(true);
            _queueServiceMock
              .Setup(x => x.RefreshQueueAsync(teamId))
              .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.DeleteSong(teamId, songId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var noContentResult = (NoContentResult)result;
            noContentResult.StatusCode.Should().Be(204);
            _songsRepositoryMock.Verify(x => x.DeleteSongAsync(teamId, songId), Times.Once);
            _queueServiceMock.Verify(x => x.RefreshQueueAsync(teamId), Times.Once);
        }

        [Fact]
        public async Task DeleteSong_WhenTeamNotFound_ShouldReturnNotFound()
        {
            // Arrange
            int teamId = 999;
            int songId = 1;

            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(teamId))
              .ReturnsAsync((Team?)null);

            // Act
            var result = await _sut.DeleteSong(teamId, songId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            notFoundResult.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task DeleteSong_WhenSongNotFound_ShouldReturnNotFound()
        {
            // Arrange
            int teamId = 1;
            int songId = 999;
            var team = new Team { Id = teamId, Name = "Test Team", CurrentSongIndex = 0, Songs = new List<Song>() };

            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(teamId))
              .ReturnsAsync(team);

            // Act
            var result = await _sut.DeleteSong(teamId, songId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            notFoundResult.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task DeleteSong_WhenDeleteFails_ShouldReturnNotFound()
        {
            // Arrange
            int teamId = 1;
            int songId = 1;
            var songToDelete = new Song { Id = 1, Title = "Song", Link = "https://youtube.com/watch?v=test", Index = 0 };
            var team = new Team
            {
                Id = teamId,
                Name = "Test Team",
                CurrentSongIndex = 0,
                Songs = new List<Song> { songToDelete }
            };

            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(teamId))
              .ReturnsAsync(team);
            _songsRepositoryMock
              .Setup(x => x.DeleteSongAsync(teamId, songId))
              .ReturnsAsync(false);

            // Act
            var result = await _sut.DeleteSong(teamId, songId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            notFoundResult.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task DeleteSong_WithMultipleSongs_ShouldShiftIndicesCorrectly()
        {
            // Arrange
            int teamId = 1;
            int songId = 2;
            var song1 = new Song { Id = 1, Title = "Song 1", Link = "https://youtube.com/watch?v=test1", Index = 0 };
            var song2 = new Song { Id = 2, Title = "Song 2", Link = "https://youtube.com/watch?v=test2", Index = 1 };
            var song3 = new Song { Id = 3, Title = "Song 3", Link = "https://youtube.com/watch?v=test3", Index = 2 };

            var team = new Team
            {
                Id = teamId,
                Name = "Test Team",
                CurrentSongIndex = 0,
                Songs = new List<Song> { song1, song2, song3 }
            };

            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(teamId))
              .ReturnsAsync(team);
            _songsRepositoryMock
              .Setup(x => x.DeleteSongAsync(teamId, songId))
              .ReturnsAsync(true);
            _songsRepositoryMock
              .Setup(x => x.UpdateSongAsync(teamId, It.IsAny<int>(), It.IsAny<Song>()))
              .ReturnsAsync((int tid, int sid, Song s) => s);
            _queueServiceMock
              .Setup(x => x.RefreshQueueAsync(teamId))
              .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.DeleteSong(teamId, songId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _songsRepositoryMock.Verify(x => x.UpdateSongAsync(teamId, 3, It.IsAny<Song>()), Times.Once);
        }

        #endregion

        #region GetQueue Tests

        [Fact]
        public async Task GetQueue_WithValidTeamId_ShouldReturnOkWithQueue()
        {
            // Arrange
            int teamId = 1;
            var queue = new List<Song>
      {
        new Song { Id = 1, Title = "Song 1", Link = "https://youtube.com/watch?v=test1", Index = 0 },
        new Song { Id = 2, Title = "Song 2", Link = "https://youtube.com/watch?v=test2", Index = 1 }
      };
            var team = new Team { Id = teamId, Name = "Test Team" };

            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(teamId))
              .ReturnsAsync(team);
            _queueServiceMock
              .Setup(x => x.GetQueueAsync(teamId))
              .ReturnsAsync(queue);

            // Act
            var result = await _sut.GetQueue(teamId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.StatusCode.Should().Be(200);
            var returnedQueue = Assert.IsAssignableFrom<IEnumerable<Song>>(okResult.Value);
            returnedQueue.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetQueue_WhenTeamNotFound_ShouldReturnNotFound()
        {
            // Arrange
            int teamId = 999;

            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(teamId))
              .ReturnsAsync((Team?)null);

            // Act
            var result = await _sut.GetQueue(teamId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            notFoundResult.StatusCode.Should().Be(404);
        }

        #endregion

        #region GetCurrentSong Tests

        [Fact]
        public async Task GetCurrentSong_WithValidTeamId_ShouldReturnOkWithCurrentSong()
        {
            // Arrange
            int teamId = 1;
            var currentSong = new Song { Id = 1, Title = "Current Song", Link = "https://youtube.com/watch?v=test", Index = 0 };
            var team = new Team { Id = teamId, Name = "Test Team" };

            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(teamId))
              .ReturnsAsync(team);
            _queueServiceMock
              .Setup(x => x.GetCurrentSongAsync(teamId))
              .ReturnsAsync(currentSong);

            // Act
            var result = await _sut.GetCurrentSong(teamId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.StatusCode.Should().Be(200);
            var returnedSong = Assert.IsType<Song>(okResult.Value);
            returnedSong.Title.Should().Be("Current Song");
        }

        [Fact]
        public async Task GetCurrentSong_WhenTeamNotFound_ShouldReturnNotFound()
        {
            // Arrange
            int teamId = 999;

            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(teamId))
              .ReturnsAsync((Team?)null);

            // Act
            var result = await _sut.GetCurrentSong(teamId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            notFoundResult.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task GetCurrentSong_WhenNoCurrentSong_ShouldReturnNotFound()
        {
            // Arrange
            int teamId = 1;
            var team = new Team { Id = teamId, Name = "Test Team" };

            _teamsRepositoryMock
              .Setup(x => x.GetByIdAsync(teamId))
              .ReturnsAsync(team);
            _queueServiceMock
              .Setup(x => x.GetCurrentSongAsync(teamId))
              .ReturnsAsync((Song?)null);

            // Act
            var result = await _sut.GetCurrentSong(teamId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            notFoundResult.StatusCode.Should().Be(404);
        }

        #endregion

        #region NextSong Tests

        [Fact]
        public async Task NextSong_WhenNextSongExists_ShouldReturnOkWithNextSong()
        {
            // Arrange
            int teamId = 1;
            var nextSong = new Song { Id = 2, Title = "Next Song", Link = "https://youtube.com/watch?v=test2", Index = 1 };

            _queueServiceMock
              .Setup(x => x.AdvanceToNextSongAsync(teamId))
              .ReturnsAsync(nextSong);

            // Act
            var result = await _sut.NextSong(teamId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.StatusCode.Should().Be(200);
            var returnedSong = Assert.IsType<Song>(okResult.Value);
            returnedSong.Title.Should().Be("Next Song");
            _queueServiceMock.Verify(x => x.AdvanceToNextSongAsync(teamId), Times.Once);
        }

        [Fact]
        public async Task NextSong_WhenNoNextSong_ShouldReturnNotFound()
        {
            // Arrange
            int teamId = 1;

            _queueServiceMock
              .Setup(x => x.AdvanceToNextSongAsync(teamId))
              .ReturnsAsync((Song?)null);

            // Act
            var result = await _sut.NextSong(teamId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            notFoundResult.StatusCode.Should().Be(404);
        }

        #endregion

        #region PreviousSong Tests

        [Fact]
        public async Task PreviousSong_WhenPreviousSongExists_ShouldReturnOkWithPreviousSong()
        {
            // Arrange
            int teamId = 1;
            var previousSong = new Song { Id = 1, Title = "Previous Song", Link = "https://youtube.com/watch?v=test1", Index = 0 };

            _queueServiceMock
              .Setup(x => x.GoToPreviousSongAsync(teamId))
              .ReturnsAsync(previousSong);

            // Act
            var result = await _sut.PreviousSong(teamId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.StatusCode.Should().Be(200);
            var returnedSong = Assert.IsType<Song>(okResult.Value);
            returnedSong.Title.Should().Be("Previous Song");
            _queueServiceMock.Verify(x => x.GoToPreviousSongAsync(teamId), Times.Once);
        }

        [Fact]
        public async Task PreviousSong_WhenAlreadyAtFirstSong_ShouldReturnNotFound()
        {
            // Arrange
            int teamId = 1;

            _queueServiceMock
              .Setup(x => x.GoToPreviousSongAsync(teamId))
              .ReturnsAsync((Song?)null);

            // Act
            var result = await _sut.PreviousSong(teamId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            notFoundResult.StatusCode.Should().Be(404);
        }

        #endregion

        #region JumpToSong Tests

        [Fact]
        public async Task JumpToSong_WithValidIndex_ShouldReturnOkWithSong()
        {
            // Arrange
            int teamId = 1;
            int index = 2;
            var targetSong = new Song { Id = 3, Title = "Target Song", Link = "https://youtube.com/watch?v=test3", Index = 2 };

            _queueServiceMock
              .Setup(x => x.JumpToSongAsync(teamId, index))
              .ReturnsAsync(targetSong);

            // Act
            var result = await _sut.JumpToSong(teamId, index);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.StatusCode.Should().Be(200);
            var returnedSong = Assert.IsType<Song>(okResult.Value);
            returnedSong.Title.Should().Be("Target Song");
            _queueServiceMock.Verify(x => x.JumpToSongAsync(teamId, index), Times.Once);
        }

        [Fact]
        public async Task JumpToSong_WithInvalidIndex_ShouldReturnNotFound()
        {
            // Arrange
            int teamId = 1;
            int index = 999;

            _queueServiceMock
              .Setup(x => x.JumpToSongAsync(teamId, index))
              .ReturnsAsync((Song?)null);

            // Act
            var result = await _sut.JumpToSong(teamId, index);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            notFoundResult.StatusCode.Should().Be(404);
        }

        #endregion
    }
}
