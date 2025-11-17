using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using back.Controllers;
using back.Models;
using back.Data.Repositories;

namespace back.Tests.Controllers
{
  public class ChatsControllerTests
  {
    private readonly Mock<IChatsRepository> _chatsRepositoryMock;
    private readonly ChatsController _sut;

    public ChatsControllerTests()
    {
      _chatsRepositoryMock = new Mock<IChatsRepository>();
      _sut = new ChatsController(_chatsRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WhenChatsRepositoryIsNull_ShouldThrowArgumentNullException()
    {
      // Act & Assert
      Assert.Throws<ArgumentNullException>(() => new ChatsController(null!));
    }

    #endregion

    #region GetMessages Tests

    [Fact]
    public async Task GetMessages_WhenMessagesExist_ShouldReturnOkWithMessages()
    {
      // Arrange
      var teamId = 1;
      var messages = new List<ChatMessage>
      {
        new ChatMessage { Id = 1, UserName = "User1", Text = "Hello", Timestamp = DateTime.UtcNow },
        new ChatMessage { Id = 2, UserName = "User2", Text = "World", Timestamp = DateTime.UtcNow }
      };

      _chatsRepositoryMock.Setup(x => x.GetMessagesAsync(teamId))
        .ReturnsAsync(messages);

      // Act
      var result = await _sut.GetMessages(teamId);

      // Assert
      var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
      var returnedMessages = okResult.Value.Should().BeAssignableTo<IEnumerable<ChatMessage>>().Subject;
      returnedMessages.Should().HaveCount(2);
      returnedMessages.Should().BeEquivalentTo(messages);
    }

    [Fact]
    public async Task GetMessages_WhenTeamNotFound_ShouldReturnNotFound()
    {
      // Arrange
      var teamId = 999;
      _chatsRepositoryMock.Setup(x => x.GetMessagesAsync(teamId))
        .ReturnsAsync((IEnumerable<ChatMessage>?)null);

      // Act
      var result = await _sut.GetMessages(teamId);

      // Assert
      var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
      notFoundResult.Value.Should().BeEquivalentTo(new { message = "Team not found" });
    }

    [Fact]
    public async Task GetMessages_WhenNoMessages_ShouldReturnOkWithEmptyList()
    {
      // Arrange
      var teamId = 1;
      var messages = new List<ChatMessage>();

      _chatsRepositoryMock.Setup(x => x.GetMessagesAsync(teamId))
        .ReturnsAsync(messages);

      // Act
      var result = await _sut.GetMessages(teamId);

      // Assert
      var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
      var returnedMessages = okResult.Value.Should().BeAssignableTo<IEnumerable<ChatMessage>>().Subject;
      returnedMessages.Should().BeEmpty();
    }

    #endregion

    #region GetMessage Tests

    [Fact]
    public async Task GetMessage_WhenMessageExists_ShouldReturnOkWithMessage()
    {
      // Arrange
      var teamId = 1;
      var messageId = 1;
      var message = new ChatMessage 
      { 
        Id = messageId, 
        UserName = "User1", 
        Text = "Hello",
        Timestamp = DateTime.UtcNow
      };

      _chatsRepositoryMock.Setup(x => x.GetMessageAsync(teamId, messageId))
        .ReturnsAsync(message);

      // Act
      var result = await _sut.GetMessage(teamId, messageId);

      // Assert
      var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
      var returnedMessage = okResult.Value.Should().BeOfType<ChatMessage>().Subject;
      returnedMessage.Should().BeEquivalentTo(message);
    }

    [Fact]
    public async Task GetMessage_WhenMessageNotFound_ShouldReturnNotFound()
    {
      // Arrange
      var teamId = 1;
      var messageId = 999;

      _chatsRepositoryMock.Setup(x => x.GetMessageAsync(teamId, messageId))
        .ReturnsAsync((ChatMessage?)null);

      // Act
      var result = await _sut.GetMessage(teamId, messageId);

      // Assert
      var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
      notFoundResult.Value.Should().BeEquivalentTo(new { message = "Team or message not found" });
    }

    [Fact]
    public async Task GetMessage_WhenTeamNotFound_ShouldReturnNotFound()
    {
      // Arrange
      var teamId = 999;
      var messageId = 1;

      _chatsRepositoryMock.Setup(x => x.GetMessageAsync(teamId, messageId))
        .ReturnsAsync((ChatMessage?)null);

      // Act
      var result = await _sut.GetMessage(teamId, messageId);

      // Assert
      var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
      notFoundResult.Value.Should().BeEquivalentTo(new { message = "Team or message not found" });
    }

    #endregion

    #region AddMessage Tests

    [Fact]
    public async Task AddMessage_WhenValidMessage_ShouldReturnCreatedAtActionWithMessage()
    {
      // Arrange
      var teamId = 1;
      var message = new ChatMessage 
      { 
        UserName = "User1", 
        Text = "Hello",
        Timestamp = DateTime.UtcNow
      };
      var createdMessage = new ChatMessage 
      { 
        Id = 1, 
        UserName = "User1", 
        Text = "Hello",
        Timestamp = message.Timestamp
      };

      _chatsRepositoryMock.Setup(x => x.AddMessageAsync(teamId, message))
        .ReturnsAsync(createdMessage);

      // Act
      var result = await _sut.AddMessage(teamId, message);

      // Assert
      var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
      createdResult.ActionName.Should().Be(nameof(ChatsController.GetMessage));
      createdResult.RouteValues.Should().ContainKey("teamId").WhoseValue.Should().Be(teamId);
      createdResult.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(createdMessage.Id);
      
      var returnedMessage = createdResult.Value.Should().BeOfType<ChatMessage>().Subject;
      returnedMessage.Should().BeEquivalentTo(createdMessage);
    }

    [Fact]
    public async Task AddMessage_WhenTeamNotFound_ShouldReturnNotFound()
    {
      // Arrange
      var teamId = 999;
      var message = new ChatMessage 
      { 
        UserName = "User1", 
        Text = "Hello",
        Timestamp = DateTime.UtcNow
      };

      _chatsRepositoryMock.Setup(x => x.AddMessageAsync(teamId, message))
        .ReturnsAsync((ChatMessage?)null);

      // Act
      var result = await _sut.AddMessage(teamId, message);

      // Assert
      var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
      notFoundResult.Value.Should().BeEquivalentTo(new { message = "Team not found" });
    }

    [Fact]
    public async Task AddMessage_WithEmptyText_ShouldStillCallRepository()
    {
      // Arrange
      var teamId = 1;
      var message = new ChatMessage 
      { 
        UserName = "User1", 
        Text = "",
        Timestamp = DateTime.UtcNow
      };
      var createdMessage = new ChatMessage 
      { 
        Id = 1, 
        UserName = "User1", 
        Text = "",
        Timestamp = message.Timestamp
      };

      _chatsRepositoryMock.Setup(x => x.AddMessageAsync(teamId, message))
        .ReturnsAsync(createdMessage);

      // Act
      var result = await _sut.AddMessage(teamId, message);

      // Assert
      var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
      _chatsRepositoryMock.Verify(x => x.AddMessageAsync(teamId, message), Times.Once);
    }

    #endregion

    #region UpdateMessage Tests

    [Fact]
    public async Task UpdateMessage_WhenValidMessage_ShouldReturnOkWithUpdatedMessage()
    {
      // Arrange
      var teamId = 1;
      var messageId = 1;
      var message = new ChatMessage 
      { 
        UserName = "User1", 
        Text = "Updated text",
        Timestamp = DateTime.UtcNow
      };
      var updatedMessage = new ChatMessage 
      { 
        Id = messageId, 
        UserName = "User1", 
        Text = "Updated text",
        Timestamp = message.Timestamp
      };

      _chatsRepositoryMock.Setup(x => x.UpdateMessageAsync(teamId, messageId, message))
        .ReturnsAsync(updatedMessage);

      // Act
      var result = await _sut.UpdateMessage(teamId, messageId, message);

      // Assert
      var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
      var returnedMessage = okResult.Value.Should().BeOfType<ChatMessage>().Subject;
      returnedMessage.Should().BeEquivalentTo(updatedMessage);
    }

    [Fact]
    public async Task UpdateMessage_WhenMessageNotFound_ShouldReturnNotFound()
    {
      // Arrange
      var teamId = 1;
      var messageId = 999;
      var message = new ChatMessage 
      { 
        UserName = "User1", 
        Text = "Updated text",
        Timestamp = DateTime.UtcNow
      };

      _chatsRepositoryMock.Setup(x => x.UpdateMessageAsync(teamId, messageId, message))
        .ReturnsAsync((ChatMessage?)null);

      // Act
      var result = await _sut.UpdateMessage(teamId, messageId, message);

      // Assert
      var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
      notFoundResult.Value.Should().BeEquivalentTo(new { message = "Team or message not found" });
    }

    [Fact]
    public async Task UpdateMessage_WhenTeamNotFound_ShouldReturnNotFound()
    {
      // Arrange
      var teamId = 999;
      var messageId = 1;
      var message = new ChatMessage 
      { 
        UserName = "User1", 
        Text = "Updated text",
        Timestamp = DateTime.UtcNow
      };

      _chatsRepositoryMock.Setup(x => x.UpdateMessageAsync(teamId, messageId, message))
        .ReturnsAsync((ChatMessage?)null);

      // Act
      var result = await _sut.UpdateMessage(teamId, messageId, message);

      // Assert
      var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
      notFoundResult.Value.Should().BeEquivalentTo(new { message = "Team or message not found" });
    }

    #endregion

    #region DeleteMessage Tests

    [Fact]
    public async Task DeleteMessage_WhenMessageExists_ShouldReturnNoContent()
    {
      // Arrange
      var teamId = 1;
      var messageId = 1;

      _chatsRepositoryMock.Setup(x => x.DeleteMessageAsync(teamId, messageId))
        .ReturnsAsync(true);

      // Act
      var result = await _sut.DeleteMessage(teamId, messageId);

      // Assert
      result.Should().BeOfType<NoContentResult>();
      _chatsRepositoryMock.Verify(x => x.DeleteMessageAsync(teamId, messageId), Times.Once);
    }

    [Fact]
    public async Task DeleteMessage_WhenMessageNotFound_ShouldReturnNotFound()
    {
      // Arrange
      var teamId = 1;
      var messageId = 999;

      _chatsRepositoryMock.Setup(x => x.DeleteMessageAsync(teamId, messageId))
        .ReturnsAsync(false);

      // Act
      var result = await _sut.DeleteMessage(teamId, messageId);

      // Assert
      var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
      notFoundResult.Value.Should().BeEquivalentTo(new { message = "Team or message not found" });
    }

    [Fact]
    public async Task DeleteMessage_WhenTeamNotFound_ShouldReturnNotFound()
    {
      // Arrange
      var teamId = 999;
      var messageId = 1;

      _chatsRepositoryMock.Setup(x => x.DeleteMessageAsync(teamId, messageId))
        .ReturnsAsync(false);

      // Act
      var result = await _sut.DeleteMessage(teamId, messageId);

      // Assert
      var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
      notFoundResult.Value.Should().BeEquivalentTo(new { message = "Team or message not found" });
    }

    #endregion

    #region Repository Verification Tests

    [Fact]
    public async Task GetMessages_ShouldCallRepositoryWithCorrectTeamId()
    {
      // Arrange
      var teamId = 42;
      _chatsRepositoryMock.Setup(x => x.GetMessagesAsync(teamId))
        .ReturnsAsync(new List<ChatMessage>());

      // Act
      await _sut.GetMessages(teamId);

      // Assert
      _chatsRepositoryMock.Verify(x => x.GetMessagesAsync(teamId), Times.Once);
    }

    [Fact]
    public async Task GetMessage_ShouldCallRepositoryWithCorrectParameters()
    {
      // Arrange
      var teamId = 42;
      var messageId = 10;
      _chatsRepositoryMock.Setup(x => x.GetMessageAsync(teamId, messageId))
        .ReturnsAsync(new ChatMessage());

      // Act
      await _sut.GetMessage(teamId, messageId);

      // Assert
      _chatsRepositoryMock.Verify(x => x.GetMessageAsync(teamId, messageId), Times.Once);
    }

    [Fact]
    public async Task AddMessage_ShouldCallRepositoryWithCorrectParameters()
    {
      // Arrange
      var teamId = 42;
      var message = new ChatMessage { UserName = "Test", Text = "Test message" };
      _chatsRepositoryMock.Setup(x => x.AddMessageAsync(teamId, message))
        .ReturnsAsync(new ChatMessage { Id = 1 });

      // Act
      await _sut.AddMessage(teamId, message);

      // Assert
      _chatsRepositoryMock.Verify(x => x.AddMessageAsync(teamId, message), Times.Once);
    }

    [Fact]
    public async Task UpdateMessage_ShouldCallRepositoryWithCorrectParameters()
    {
      // Arrange
      var teamId = 42;
      var messageId = 10;
      var message = new ChatMessage { UserName = "Test", Text = "Updated message" };
      _chatsRepositoryMock.Setup(x => x.UpdateMessageAsync(teamId, messageId, message))
        .ReturnsAsync(new ChatMessage { Id = messageId });

      // Act
      await _sut.UpdateMessage(teamId, messageId, message);

      // Assert
      _chatsRepositoryMock.Verify(x => x.UpdateMessageAsync(teamId, messageId, message), Times.Once);
    }

    [Fact]
    public async Task DeleteMessage_ShouldCallRepositoryWithCorrectParameters()
    {
      // Arrange
      var teamId = 42;
      var messageId = 10;
      _chatsRepositoryMock.Setup(x => x.DeleteMessageAsync(teamId, messageId))
        .ReturnsAsync(true);

      // Act
      await _sut.DeleteMessage(teamId, messageId);

      // Assert
      _chatsRepositoryMock.Verify(x => x.DeleteMessageAsync(teamId, messageId), Times.Once);
    }

    #endregion

    #region Timestamp Tests

    [Fact]
    public async Task AddMessage_ShouldPreserveTimestamp()
    {
      // Arrange
      var teamId = 1;
      var timestamp = DateTime.UtcNow.AddMinutes(-5);
      var message = new ChatMessage 
      { 
        UserName = "User1", 
        Text = "Hello",
        Timestamp = timestamp
      };
      var createdMessage = new ChatMessage 
      { 
        Id = 1, 
        UserName = "User1", 
        Text = "Hello",
        Timestamp = timestamp
      };

      _chatsRepositoryMock.Setup(x => x.AddMessageAsync(teamId, message))
        .ReturnsAsync(createdMessage);

      // Act
      var result = await _sut.AddMessage(teamId, message);

      // Assert
      var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
      var returnedMessage = createdResult.Value.Should().BeOfType<ChatMessage>().Subject;
      returnedMessage.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public async Task GetMessages_ShouldReturnMessagesWithTimestamps()
    {
      // Arrange
      var teamId = 1;
      var timestamp1 = DateTime.UtcNow.AddMinutes(-10);
      var timestamp2 = DateTime.UtcNow.AddMinutes(-5);
      var messages = new List<ChatMessage>
      {
        new ChatMessage { Id = 1, UserName = "User1", Text = "First", Timestamp = timestamp1 },
        new ChatMessage { Id = 2, UserName = "User2", Text = "Second", Timestamp = timestamp2 }
      };

      _chatsRepositoryMock.Setup(x => x.GetMessagesAsync(teamId))
        .ReturnsAsync(messages);

      // Act
      var result = await _sut.GetMessages(teamId);

      // Assert
      var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
      var returnedMessages = okResult.Value.Should().BeAssignableTo<IEnumerable<ChatMessage>>().Subject.ToList();
      returnedMessages[0].Timestamp.Should().Be(timestamp1);
      returnedMessages[1].Timestamp.Should().Be(timestamp2);
    }

    #endregion
  }
}