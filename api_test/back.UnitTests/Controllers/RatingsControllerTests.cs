using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using back.Controllers;
using back.Models;
using back.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace back.Tests.Controllers
{
    public class RatingsControllerTests
    {
        private readonly Mock<IRatingsRepository> _ratingsRepositoryMock;
        private readonly Mock<ISongsRepository> _songsRepositoryMock;
        private readonly Mock<ILogger<RatingsController>> _loggerMock;
        private readonly RatingsController _sut;

        public RatingsControllerTests()
        {
            _ratingsRepositoryMock = new Mock<IRatingsRepository>();
            _songsRepositoryMock = new Mock<ISongsRepository>();
            _loggerMock = new Mock<ILogger<RatingsController>>();

            _sut = new RatingsController(
              _ratingsRepositoryMock.Object,
              _songsRepositoryMock.Object,
              _loggerMock.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WhenRatingsRepositoryIsNull_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
              new RatingsController(
                null!,
                _songsRepositoryMock.Object,
                _loggerMock.Object));
        }

        [Fact]
        public void Constructor_WhenSongsRepositoryIsNull_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
              new RatingsController(
                _ratingsRepositoryMock.Object,
                null!,
                _loggerMock.Object));
        }

        [Fact]
        public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
              new RatingsController(
                _ratingsRepositoryMock.Object,
                _songsRepositoryMock.Object,
                null!));
        }

        #endregion

        #region GetRatings Tests

        [Fact]
        public async Task GetRatings_WithValidSongId_ShouldReturnOkWithRatings()
        {
            // Arrange
            int teamId = 1;
            int songId = 1;
            var song = new Song { Id = songId, Title = "Test Song", Link = "https://youtube.com/watch?v=test" };
            var ratings = new List<SongRating>
      {
        new SongRating { Id = 1, SongId = songId, UserId = 1, Rating = 80 },
        new SongRating { Id = 2, SongId = songId, UserId = 2, Rating = 90 }
      };

            _songsRepositoryMock
              .Setup(x => x.GetSongAsync(teamId, songId))
              .ReturnsAsync(song);
            _ratingsRepositoryMock
              .Setup(x => x.GetSongRatingsAsync(teamId, songId))
              .ReturnsAsync(ratings);

            // Act
            var result = await _sut.GetRatings(teamId, songId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.StatusCode.Should().Be(200);
            var returnedRatings = Assert.IsAssignableFrom<IEnumerable<SongRating>>(okResult.Value);
            returnedRatings.Should().HaveCount(2);
            _songsRepositoryMock.Verify(x => x.GetSongAsync(teamId, songId), Times.Once);
            _ratingsRepositoryMock.Verify(x => x.GetSongRatingsAsync(teamId, songId), Times.Once);
        }

        [Fact]
        public async Task GetRatings_WhenSongNotFound_ShouldReturnNotFound()
        {
            // Arrange
            int teamId = 1;
            int songId = 999;

            _songsRepositoryMock
              .Setup(x => x.GetSongAsync(teamId, songId))
              .ReturnsAsync((Song?)null);

            // Act
            var result = await _sut.GetRatings(teamId, songId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            notFoundResult.StatusCode.Should().Be(404);
            _ratingsRepositoryMock.Verify(x => x.GetSongRatingsAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetRatings_WithNoRatings_ShouldReturnOkWithEmptyList()
        {
            // Arrange
            int teamId = 1;
            int songId = 1;
            var song = new Song { Id = songId, Title = "Test Song", Link = "https://youtube.com/watch?v=test" };
            var emptyRatings = new List<SongRating>();

            _songsRepositoryMock
              .Setup(x => x.GetSongAsync(teamId, songId))
              .ReturnsAsync(song);
            _ratingsRepositoryMock
              .Setup(x => x.GetSongRatingsAsync(teamId, songId))
              .ReturnsAsync(emptyRatings);

            // Act
            var result = await _sut.GetRatings(teamId, songId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedRatings = Assert.IsAssignableFrom<IEnumerable<SongRating>>(okResult.Value);
            returnedRatings.Should().BeEmpty();
        }

        #endregion

        #region SubmitRating Tests

        [Fact]
        public async Task SubmitRating_WithNewRating_ShouldAddRatingAndReturnOk()
        {
            // Arrange
            int teamId = 1;
            int songId = 1;
            int userId = 1;
            var song = new Song { Id = songId, Title = "Test Song", Link = "https://youtube.com/watch?v=test" };
            var incomingRating = new SongRating { UserId = userId, Rating = 85 };
            var createdRating = new SongRating { Id = 1, SongId = songId, UserId = userId, Rating = 85, CreatedAt = DateTime.UtcNow };

            _songsRepositoryMock
              .Setup(x => x.GetSongAsync(teamId, songId))
              .ReturnsAsync(song);
            _ratingsRepositoryMock
              .Setup(x => x.GetUserRatingAsync(teamId, songId, userId))
              .ReturnsAsync((SongRating?)null);
            _ratingsRepositoryMock
              .Setup(x => x.AddRatingAsync(teamId, songId, It.IsAny<SongRating>()))
              .ReturnsAsync(createdRating);

            // Act
            var result = await _sut.SubmitRating(teamId, songId, incomingRating);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.StatusCode.Should().Be(200);
            var returnedRating = Assert.IsType<SongRating>(okResult.Value);
            returnedRating.Id.Should().Be(1);
            returnedRating.Rating.Should().Be(85);
            _ratingsRepositoryMock.Verify(x => x.AddRatingAsync(teamId, songId, It.IsAny<SongRating>()), Times.Once);
            _ratingsRepositoryMock.Verify(x => x.UpdateRatingAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SongRating>()), Times.Never);
        }

        [Fact]
        public async Task SubmitRating_WithExistingRating_ShouldUpdateRatingAndReturnOk()
        {
            // Arrange
            int teamId = 1;
            int songId = 1;
            int userId = 1;
            int existingRatingId = 1;
            var song = new Song { Id = songId, Title = "Test Song", Link = "https://youtube.com/watch?v=test" };
            var incomingRating = new SongRating { UserId = userId, Rating = 95 };
            var existingRating = new SongRating { Id = existingRatingId, SongId = songId, UserId = userId, Rating = 85 };
            var updatedRating = new SongRating { Id = existingRatingId, SongId = songId, UserId = userId, Rating = 95, UpdatedAt = DateTime.UtcNow };

            _songsRepositoryMock
              .Setup(x => x.GetSongAsync(teamId, songId))
              .ReturnsAsync(song);
            _ratingsRepositoryMock
              .Setup(x => x.GetUserRatingAsync(teamId, songId, userId))
              .ReturnsAsync(existingRating);
            _ratingsRepositoryMock
              .Setup(x => x.UpdateRatingAsync(teamId, songId, existingRatingId, It.IsAny<SongRating>()))
              .ReturnsAsync(updatedRating);

            // Act
            var result = await _sut.SubmitRating(teamId, songId, incomingRating);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.StatusCode.Should().Be(200);
            var returnedRating = Assert.IsType<SongRating>(okResult.Value);
            returnedRating.Rating.Should().Be(95);
            _ratingsRepositoryMock.Verify(x => x.UpdateRatingAsync(teamId, songId, existingRatingId, It.IsAny<SongRating>()), Times.Once);
            _ratingsRepositoryMock.Verify(x => x.AddRatingAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SongRating>()), Times.Never);
        }

        [Fact]
        public async Task SubmitRating_WhenSongNotFound_ShouldReturnNotFound()
        {
            // Arrange
            int teamId = 1;
            int songId = 999;
            var incomingRating = new SongRating { UserId = 1, Rating = 85 };

            _songsRepositoryMock
              .Setup(x => x.GetSongAsync(teamId, songId))
              .ReturnsAsync((Song?)null);

            // Act
            var result = await _sut.SubmitRating(teamId, songId, incomingRating);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            notFoundResult.StatusCode.Should().Be(404);
            _ratingsRepositoryMock.Verify(x => x.GetUserRatingAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task SubmitRating_WhenAddFails_ShouldReturnBadRequest()
        {
            // Arrange
            int teamId = 1;
            int songId = 1;
            int userId = 1;
            var song = new Song { Id = songId, Title = "Test Song", Link = "https://youtube.com/watch?v=test" };
            var incomingRating = new SongRating { UserId = userId, Rating = 85 };

            _songsRepositoryMock
              .Setup(x => x.GetSongAsync(teamId, songId))
              .ReturnsAsync(song);
            _ratingsRepositoryMock
              .Setup(x => x.GetUserRatingAsync(teamId, songId, userId))
              .ReturnsAsync((SongRating?)null);
            _ratingsRepositoryMock
              .Setup(x => x.AddRatingAsync(teamId, songId, It.IsAny<SongRating>()))
              .ReturnsAsync((SongRating?)null);

            // Act
            var result = await _sut.SubmitRating(teamId, songId, incomingRating);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            badRequestResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task SubmitRating_WhenUpdateFails_ShouldReturnBadRequest()
        {
            // Arrange
            int teamId = 1;
            int songId = 1;
            int userId = 1;
            int existingRatingId = 1;
            var song = new Song { Id = songId, Title = "Test Song", Link = "https://youtube.com/watch?v=test" };
            var incomingRating = new SongRating { UserId = userId, Rating = 95 };
            var existingRating = new SongRating { Id = existingRatingId, SongId = songId, UserId = userId, Rating = 85 };

            _songsRepositoryMock
              .Setup(x => x.GetSongAsync(teamId, songId))
              .ReturnsAsync(song);
            _ratingsRepositoryMock
              .Setup(x => x.GetUserRatingAsync(teamId, songId, userId))
              .ReturnsAsync(existingRating);
            _ratingsRepositoryMock
              .Setup(x => x.UpdateRatingAsync(teamId, songId, existingRatingId, It.IsAny<SongRating>()))
              .ReturnsAsync((SongRating?)null);

            // Act
            var result = await _sut.SubmitRating(teamId, songId, incomingRating);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            badRequestResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task SubmitRating_ShouldSetSongIdAndUpdatedAt()
        {
            // Arrange
            int teamId = 1;
            int songId = 1;
            int userId = 1;
            var song = new Song { Id = songId, Title = "Test Song", Link = "https://youtube.com/watch?v=test" };
            var incomingRating = new SongRating { UserId = userId, Rating = 85, SongId = 0 };
            var createdRating = new SongRating { Id = 1, SongId = songId, UserId = userId, Rating = 85 };
            var beforeTime = DateTime.UtcNow;

            _songsRepositoryMock
              .Setup(x => x.GetSongAsync(teamId, songId))
              .ReturnsAsync(song);
            _ratingsRepositoryMock
              .Setup(x => x.GetUserRatingAsync(teamId, songId, userId))
              .ReturnsAsync((SongRating?)null);
            _ratingsRepositoryMock
              .Setup(x => x.AddRatingAsync(teamId, songId, It.IsAny<SongRating>()))
              .Returns((int tid, int sid, SongRating r) =>
              {
                  var result = new SongRating { Id = 1, SongId = r.SongId, UserId = r.UserId, Rating = r.Rating, CreatedAt = r.CreatedAt, UpdatedAt = r.UpdatedAt };
                  return Task.FromResult((SongRating?)result);
              });

            // Act
            await _sut.SubmitRating(teamId, songId, incomingRating);

            // Assert
            _ratingsRepositoryMock.Verify(
              x => x.AddRatingAsync(teamId, songId, It.Is<SongRating>(r =>
                r.SongId == songId &&
                r.UpdatedAt >= beforeTime)), Times.Once);
        }

        #endregion

        #region DeleteRating Tests

        [Fact]
        public async Task DeleteRating_WithValidId_ShouldReturnNoContent()
        {
            // Arrange
            int teamId = 1;
            int songId = 1;
            int ratingId = 1;

            _ratingsRepositoryMock
              .Setup(x => x.DeleteRatingAsync(teamId, songId, ratingId))
              .ReturnsAsync(true);

            // Act
            var result = await _sut.DeleteRating(teamId, songId, ratingId);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            noContentResult.StatusCode.Should().Be(204);
            _ratingsRepositoryMock.Verify(x => x.DeleteRatingAsync(teamId, songId, ratingId), Times.Once);
        }

        [Fact]
        public async Task DeleteRating_WhenRatingNotFound_ShouldReturnNotFound()
        {
            // Arrange
            int teamId = 1;
            int songId = 1;
            int ratingId = 999;

            _ratingsRepositoryMock
              .Setup(x => x.DeleteRatingAsync(teamId, songId, ratingId))
              .ReturnsAsync(false);

            // Act
            var result = await _sut.DeleteRating(teamId, songId, ratingId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            notFoundResult.StatusCode.Should().Be(404);
        }

        #endregion
    }
}
