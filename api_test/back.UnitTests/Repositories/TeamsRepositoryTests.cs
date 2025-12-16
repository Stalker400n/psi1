using FluentAssertions;
using back.Models;
using back.Data;
using back.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace back.Tests.Repositories
{
    public class TeamsRepositoryTests : IAsyncLifetime
    {
        private ApplicationDbContext _context = null!;
        private TeamsRepository _sut = null!;

        public async Task InitializeAsync()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
              .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
              .Options;

            _context = new ApplicationDbContext(options);
            await _context.Database.EnsureCreatedAsync();
            _sut = new TeamsRepository(_context);
        }

        public async Task DisposeAsync()
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.DisposeAsync();
        }

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WhenTeamIsValid_ShouldCreateAndReturnTeam()
        {
            // Arrange
            var team = new Team
            {
                Name = "Test Team",
                CurrentSongIndex = 0,
                Songs = new List<Song>()
            };

            // Act
            var result = await _sut.CreateAsync(team);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.Name.Should().Be("Test Team");
            result.CurrentSongIndex.Should().Be(0);

            // Verify in database
            var dbTeam = await _context.Teams.FirstOrDefaultAsync(t => t.Id == result.Id);
            dbTeam.Should().NotBeNull();
            dbTeam!.Name.Should().Be("Test Team");
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WhenTeamExists_ShouldReturnTeam()
        {
            // Arrange
            var team = new Team
            {
                Name = "Existing Team",
                CurrentSongIndex = 0,
                Songs = new List<Song>()
            };
            var createdTeam = await _sut.CreateAsync(team);

            // Act
            var result = await _sut.GetByIdAsync(createdTeam.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(createdTeam.Id);
            result.Name.Should().Be("Existing Team");
        }

        [Fact]
        public async Task GetByIdAsync_WhenTeamDoesNotExist_ShouldReturnNull()
        {
            // Act
            var result = await _sut.GetByIdAsync(9999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WhenTeamsExist_ShouldReturnAllTeams()
        {
            // Arrange
            var team1 = new Team { Name = "Team 1", CurrentSongIndex = 0, Songs = new List<Song>() };
            var team2 = new Team { Name = "Team 2", CurrentSongIndex = 0, Songs = new List<Song>() };

            await _sut.CreateAsync(team1);
            await _sut.CreateAsync(team2);

            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCountGreaterThanOrEqualTo(2);
            result.Should().Contain(t => t.Name == "Team 1");
            result.Should().Contain(t => t.Name == "Team 2");
        }

        [Fact]
        public async Task GetAllAsync_WhenNoTeamsExist_ShouldReturnEmptyList()
        {
            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WhenTeamExists_ShouldUpdateAndReturnTeam()
        {
            // Arrange
            var team = new Team { Name = "Original Name", CurrentSongIndex = 0, Songs = new List<Song>() };
            var createdTeam = await _sut.CreateAsync(team);

            var updateTeam = new Team
            {
                Id = createdTeam.Id,
                Name = "Updated Name",
                CurrentSongIndex = 0,
                Songs = new List<Song>()
            };

            // Act
            var result = await _sut.UpdateAsync(createdTeam.Id, updateTeam);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Updated Name");

            // Verify in database
            var dbTeam = await _context.Teams.FirstOrDefaultAsync(t => t.Id == createdTeam.Id);
            dbTeam!.Name.Should().Be("Updated Name");
        }

        [Fact]
        public async Task UpdateAsync_WhenTeamDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var team = new Team { Name = "Test Team", CurrentSongIndex = 0, Songs = new List<Song>() };

            // Act
            var result = await _sut.UpdateAsync(9999, team);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WhenTeamExists_ShouldDeleteTeam()
        {
            // Arrange
            var team = new Team { Name = "Team to Delete", CurrentSongIndex = 0, Songs = new List<Song>() };
            var createdTeam = await _sut.CreateAsync(team);

            // Act
            var result = await _sut.DeleteAsync(createdTeam.Id);

            // Assert
            result.Should().BeTrue();

            // Verify in database
            var dbTeam = await _context.Teams.FirstOrDefaultAsync(t => t.Id == createdTeam.Id);
            dbTeam.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_WhenTeamDoesNotExist_ShouldReturnFalse()
        {
            // Act
            var result = await _sut.DeleteAsync(9999);

            // Assert
            result.Should().BeFalse();
        }

        #endregion
    }
}
