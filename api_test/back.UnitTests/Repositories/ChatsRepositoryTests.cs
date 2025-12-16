using FluentAssertions;
using back.Models;
using back.Data;
using back.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace back.Tests.Repositories
{
    public class ChatsRepositoryTests : IAsyncLifetime
    {
        private ApplicationDbContext _context = null!;
        private ChatsRepository _sut = null!;
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

            _sut = new ChatsRepository(_context);
        }

        public async Task DisposeAsync()
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.DisposeAsync();
        }

        #region AddMessageAsync Tests

        [Fact]
        public async Task AddMessageAsync_WhenValidMessage_ShouldCreateAndReturnMessage()
        {
            // Arrange
            var message = new ChatMessage
            {
                Text = "Test Message",
                UserName = "TestUser",
                Timestamp = DateTime.UtcNow
            };

            // Act
            var result = await _sut.AddMessageAsync(_testTeam.Id, message);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().BeGreaterThan(0);
            result.Text.Should().Be("Test Message");
            result.UserName.Should().Be("TestUser");
        }

        #endregion

        #region GetMessagesAsync Tests

        [Fact]
        public async Task GetMessagesAsync_WhenMessagesExist_ShouldReturnAllMessages()
        {
            // Arrange
            var message1 = new ChatMessage { Text = "Message 1", UserName = "User1", Timestamp = DateTime.UtcNow };
            var message2 = new ChatMessage { Text = "Message 2", UserName = "User2", Timestamp = DateTime.UtcNow };

            await _sut.AddMessageAsync(_testTeam.Id, message1);
            await _sut.AddMessageAsync(_testTeam.Id, message2);

            // Act
            var result = await _sut.GetMessagesAsync(_testTeam.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCountGreaterThanOrEqualTo(2);
            result.Should().Contain(m => m.Text == "Message 1");
            result.Should().Contain(m => m.Text == "Message 2");
        }

        [Fact]
        public async Task GetMessagesAsync_WhenNoMessagesExist_ShouldReturnEmpty()
        {
            // Act
            var result = await _sut.GetMessagesAsync(_testTeam.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetMessagesAsync_WhenTeamNotFound_ShouldReturnNull()
        {
            // Act
            var result = await _sut.GetMessagesAsync(9999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetMessageAsync Tests

        [Fact]
        public async Task GetMessageAsync_WhenMessageExists_ShouldReturnMessage()
        {
            // Arrange
            var message = new ChatMessage { Text = "Test", UserName = "TestUser", Timestamp = DateTime.UtcNow };
            var createdMessage = await _sut.AddMessageAsync(_testTeam.Id, message);

            // Act
            var result = await _sut.GetMessageAsync(_testTeam.Id, createdMessage!.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(createdMessage.Id);
            result.Text.Should().Be("Test");
        }

        [Fact]
        public async Task GetMessageAsync_WhenMessageNotFound_ShouldReturnNull()
        {
            // Act
            var result = await _sut.GetMessageAsync(_testTeam.Id, 9999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region UpdateMessageAsync Tests

        [Fact]
        public async Task UpdateMessageAsync_WhenMessageExists_ShouldUpdateAndReturnMessage()
        {
            // Arrange
            var message = new ChatMessage { Text = "Original", UserName = "TestUser", Timestamp = DateTime.UtcNow };
            var createdMessage = await _sut.AddMessageAsync(_testTeam.Id, message);

            var updatedMessage = new ChatMessage
            {
                Id = createdMessage!.Id,
                Text = "Updated",
                UserName = "TestUser",
                Timestamp = DateTime.UtcNow
            };

            // Act
            var result = await _sut.UpdateMessageAsync(_testTeam.Id, createdMessage.Id, updatedMessage);

            // Assert
            result.Should().NotBeNull();
            result!.Text.Should().Be("Updated");
        }

        [Fact]
        public async Task UpdateMessageAsync_WhenMessageNotFound_ShouldReturnNull()
        {
            // Arrange
            var message = new ChatMessage { Text = "Message", UserName = "TestUser", Timestamp = DateTime.UtcNow };

            // Act
            var result = await _sut.UpdateMessageAsync(_testTeam.Id, 9999, message);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region DeleteMessageAsync Tests

        [Fact]
        public async Task DeleteMessageAsync_WhenMessageExists_ShouldDeleteMessage()
        {
            // Arrange
            var message = new ChatMessage { Text = "Message", UserName = "TestUser", Timestamp = DateTime.UtcNow };
            var createdMessage = await _sut.AddMessageAsync(_testTeam.Id, message);

            // Act
            var result = await _sut.DeleteMessageAsync(_testTeam.Id, createdMessage!.Id);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteMessageAsync_WhenMessageNotFound_ShouldReturnFalse()
        {
            // Act
            var result = await _sut.DeleteMessageAsync(_testTeam.Id, 9999);

            // Assert
            result.Should().BeFalse();
        }

        #endregion
    }
}
