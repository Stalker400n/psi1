using FluentAssertions;
using back.Models;
using back.Data;
using back.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace back.Tests.Repositories
{
    public class SongsRepositoryTests : IAsyncLifetime
    {
        private ApplicationDbContext _context = null!;
        private SongsRepository _sut = null!;
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

            _sut = new SongsRepository(_context);
        }

        public async Task DisposeAsync()
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.DisposeAsync();
        }

        #region AddSongAsync Tests

        [Fact]
        public async Task AddSongAsync_WhenValidSong_ShouldCreateAndReturnSong()
        {
            // Arrange
            var song = new Song
            {
                Title = "Test Song",
                Artist = "Test Artist",
                Link = "https://youtube.com/watch?v=test",
                Index = 0,
                DurationSeconds = 180
            };

            // Act
            var result = await _sut.AddSongAsync(_testTeam.Id, song);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().BeGreaterThan(0);
            result.Title.Should().Be("Test Song");
            result.Artist.Should().Be("Test Artist");

            // Verify in database
            var dbSong = await _context.Songs.FirstOrDefaultAsync(s => s.Id == result.Id);
            dbSong.Should().NotBeNull();
            dbSong!.Title.Should().Be("Test Song");
        }

        #endregion

        #region GetSongsAsync Tests

        [Fact]
        public async Task GetSongsAsync_WhenSongsExist_ShouldReturnAllSongs()
        {
            // Arrange
            var song1 = new Song
            {
                Title = "Song 1",
                Artist = "Artist 1",
                Link = "https://youtube.com/watch?v=test1",
                Index = 0,
                DurationSeconds = 180
            };
            var song2 = new Song
            {
                Title = "Song 2",
                Artist = "Artist 2",
                Link = "https://youtube.com/watch?v=test2",
                Index = 1,
                DurationSeconds = 200
            };

            var createdSong1 = await _sut.AddSongAsync(_testTeam.Id, song1);
            var createdSong2 = await _sut.AddSongAsync(_testTeam.Id, song2);

            // Act
            var result = await _sut.GetSongsAsync(_testTeam.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCountGreaterThanOrEqualTo(2);
            result.Should().Contain(s => s.Title == "Song 1");
            result.Should().Contain(s => s.Title == "Song 2");
        }

        [Fact]
        public async Task GetSongsAsync_WhenNoSongsExist_ShouldReturnEmpty()
        {
            // Act
            var result = await _sut.GetSongsAsync(_testTeam.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetSongsAsync_WhenTeamNotFound_ShouldReturnNull()
        {
            // Act
            var result = await _sut.GetSongsAsync(9999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetSongAsync Tests

        [Fact]
        public async Task GetSongAsync_WhenSongExists_ShouldReturnSong()
        {
            // Arrange
            var song = new Song { Title = "Song", Artist = "Artist", Link = "https://youtube.com/watch?v=test", Index = 0, DurationSeconds = 180 };
            var createdSong = await _sut.AddSongAsync(_testTeam.Id, song);

            // Act
            var result = await _sut.GetSongAsync(_testTeam.Id, createdSong!.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(createdSong.Id);
            result.Title.Should().Be("Song");
        }

        [Fact]
        public async Task GetSongAsync_WhenSongNotFound_ShouldReturnNull()
        {
            // Act
            var result = await _sut.GetSongAsync(_testTeam.Id, 9999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region UpdateSongAsync Tests

        [Fact]
        public async Task UpdateSongAsync_WhenSongExists_ShouldUpdateAndReturnSong()
        {
            // Arrange
            var song = new Song { Title = "Original", Artist = "Artist", Link = "https://youtube.com/watch?v=test", Index = 0, DurationSeconds = 180 };
            var createdSong = await _sut.AddSongAsync(_testTeam.Id, song);

            var updatedSong = new Song
            {
                Title = "Updated",
                Artist = "Updated Artist",
                Link = "https://youtube.com/watch?v=test",
                Index = 1,
                DurationSeconds = 180
            };

            // Act
            var result = await _sut.UpdateSongAsync(_testTeam.Id, createdSong!.Id, updatedSong);

            // Assert
            result.Should().NotBeNull();
            // UpdateSongAsync only updates Rating and Index properties
            result!.Index.Should().Be(1);
            result.Title.Should().Be("Original"); // Title is NOT updated
            result.Artist.Should().Be("Artist"); // Artist is NOT updated
        }

        [Fact]
        public async Task UpdateSongAsync_WhenSongNotFound_ShouldReturnNull()
        {
            // Arrange
            var song = new Song { Title = "Song", Artist = "Artist", Link = "https://youtube.com/watch?v=test", Index = 0, DurationSeconds = 180 };

            // Act
            var result = await _sut.UpdateSongAsync(_testTeam.Id, 9999, song);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region DeleteSongAsync Tests

        [Fact]
        public async Task DeleteSongAsync_WhenSongExists_ShouldDeleteSong()
        {
            // Arrange
            var song = new Song { Title = "Song", Artist = "Artist", Link = "https://youtube.com/watch?v=test", Index = 0, DurationSeconds = 180 };
            var createdSong = await _sut.AddSongAsync(_testTeam.Id, song);

            // Act
            var result = await _sut.DeleteSongAsync(_testTeam.Id, createdSong!.Id);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteSongAsync_WhenSongNotFound_ShouldReturnFalse()
        {
            // Act
            var result = await _sut.DeleteSongAsync(_testTeam.Id, 9999);

            // Assert
            result.Should().BeFalse();
        }

        #endregion
    }
}
