using FluentAssertions;
using back.Models;
using back.Data;
using back.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace back.Tests.Repositories
{
    public class RatingsRepositoryTests : IAsyncLifetime
    {
        private ApplicationDbContext _context = null!;
        private RatingsRepository _sut = null!;
        private Team _testTeam = null!;
        private Song _testSong = null!;
        private User _testUser = null!;

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

            // Create test song
            _testSong = new Song
            {
                Title = "Test Song",
                Artist = "Test Artist",
                Link = "https://youtube.com/watch?v=test",
                Index = 0,
                DurationSeconds = 180
            };
            _testTeam.Songs.Add(_testSong);
            _context.SaveChanges();

            // Create test user
            _testUser = new User
            {
                Name = "Test User",
                Score = 100,
                IsActive = true
            };
            _context.Users.Add(_testUser);
            await _context.SaveChangesAsync();

            _sut = new RatingsRepository(_context);
        }

        public async Task DisposeAsync()
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.DisposeAsync();
        }

        #region AddRatingAsync Tests

        [Fact]
        public async Task AddRatingAsync_WhenValidRating_ShouldCreateAndReturnRating()
        {
            // Arrange
            var rating = new SongRating
            {
                Rating = 50,
                UserId = _testUser.Id,
                SongId = _testSong.Id
            };

            // Act
            var result = await _sut.AddRatingAsync(_testTeam.Id, _testSong.Id, rating);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().BeGreaterThan(0);
            result.Rating.Should().Be(50);
            result.UserId.Should().Be(_testUser.Id);
        }

        #endregion

        #region GetSongRatingsAsync Tests

        [Fact]
        public async Task GetSongRatingsAsync_WhenRatingsExist_ShouldReturnAllRatings()
        {
            // Arrange
            var user2 = new User { Name = "User 2", Score = 100, IsActive = true };
            _context.Users.Add(user2);
            await _context.SaveChangesAsync();

            var rating1 = new SongRating { Rating = 50, UserId = _testUser.Id, SongId = _testSong.Id };
            var rating2 = new SongRating { Rating = 75, UserId = user2.Id, SongId = _testSong.Id };

            _context.SongRatings.Add(rating1);
            _context.SongRatings.Add(rating2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetSongRatingsAsync(_testTeam.Id, _testSong.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCountGreaterThanOrEqualTo(2);
            result.Should().Contain(r => r.Rating == 50);
            result.Should().Contain(r => r.Rating == 75);
        }

        [Fact]
        public async Task GetSongRatingsAsync_WhenNoRatingsExist_ShouldReturnEmpty()
        {
            // Act
            var result = await _sut.GetSongRatingsAsync(_testTeam.Id, _testSong.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region GetUserRatingAsync Tests

        [Fact]
        public async Task GetUserRatingAsync_WhenRatingExists_ShouldReturnRating()
        {
            // Arrange
            var rating = new SongRating { Rating = 50, UserId = _testUser.Id, SongId = _testSong.Id };
            _context.SongRatings.Add(rating);
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetUserRatingAsync(_testTeam.Id, _testSong.Id, _testUser.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Rating.Should().Be(50);
            result.UserId.Should().Be(_testUser.Id);
        }

        [Fact]
        public async Task GetUserRatingAsync_WhenRatingNotFound_ShouldReturnNull()
        {
            // Act
            var result = await _sut.GetUserRatingAsync(_testTeam.Id, _testSong.Id, 9999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region UpdateRatingAsync Tests

        [Fact]
        public async Task UpdateRatingAsync_WhenRatingExists_ShouldUpdateAndReturnRating()
        {
            // Arrange
            var rating = new SongRating { Rating = 30, UserId = _testUser.Id, SongId = _testSong.Id };
            _context.SongRatings.Add(rating);
            await _context.SaveChangesAsync();

            var updatedRating = new SongRating
            {
                Id = rating.Id,
                Rating = 80,
                UserId = _testUser.Id,
                SongId = _testSong.Id
            };

            // Act
            var result = await _sut.UpdateRatingAsync(_testTeam.Id, _testSong.Id, rating.Id, updatedRating);

            // Assert
            result.Should().NotBeNull();
            result!.Rating.Should().Be(80);
        }

        [Fact]
        public async Task UpdateRatingAsync_WhenRatingNotFound_ShouldReturnNull()
        {
            // Arrange
            var rating = new SongRating { Rating = 50, UserId = _testUser.Id, SongId = _testSong.Id };

            // Act
            var result = await _sut.UpdateRatingAsync(_testTeam.Id, _testSong.Id, 9999, rating);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region DeleteRatingAsync Tests

        [Fact]
        public async Task DeleteRatingAsync_WhenRatingExists_ShouldDeleteRating()
        {
            // Arrange
            var rating = new SongRating { Rating = 50, UserId = _testUser.Id, SongId = _testSong.Id };
            _context.SongRatings.Add(rating);
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.DeleteRatingAsync(_testTeam.Id, _testSong.Id, rating.Id);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteRatingAsync_WhenRatingNotFound_ShouldReturnFalse()
        {
            // Act
            var result = await _sut.DeleteRatingAsync(_testTeam.Id, _testSong.Id, 9999);

            // Assert
            result.Should().BeFalse();
        }

        #endregion
    }
}
