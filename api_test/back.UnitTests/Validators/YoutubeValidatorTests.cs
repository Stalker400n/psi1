using Xunit;
using Moq;
using Moq.Protected;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using back.Validators;
using back.Exceptions;
using System.Net;

namespace back.Tests.Validators
{
  public class YoutubeValidatorTests
  {
    private readonly Mock<ILogger<YoutubeValidator>> _loggerMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly YoutubeValidator _sut;

    public YoutubeValidatorTests()
    {
      _loggerMock = new Mock<ILogger<YoutubeValidator>>();
      _httpClientFactoryMock = new Mock<IHttpClientFactory>();
      _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

      var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
      _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>()))
        .Returns(httpClient);

      _sut = new YoutubeValidator(_loggerMock.Object, _httpClientFactoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
      // Act & Assert
      Assert.Throws<ArgumentNullException>(() =>
        new YoutubeValidator(null!, _httpClientFactoryMock.Object));
    }

    [Fact]
    public void Constructor_WhenHttpClientFactoryIsNull_ShouldThrowArgumentNullException()
    {
      // Act & Assert
      Assert.Throws<ArgumentNullException>(() =>
        new YoutubeValidator(_loggerMock.Object, null!));
    }

    #endregion

    #region ValidateLink - Empty/Null Link Tests

    [Fact]
    public async Task ValidateLink_WhenLinkIsNull_ShouldThrowYoutubeValidationException()
    {
      // Act & Assert
      var exception = await Assert.ThrowsAsync<YoutubeValidationException>(() =>
        _sut.ValidateLink(null!));

      exception.Message.Should().Contain("YouTube link cannot be empty");
      exception.InvalidLink.Should().BeNull();
    }

    [Fact]
    public async Task ValidateLink_WhenLinkIsEmpty_ShouldThrowYoutubeValidationException()
    {
      // Arrange
      var link = "";

      // Act & Assert
      var exception = await Assert.ThrowsAsync<YoutubeValidationException>(() =>
        _sut.ValidateLink(link));

      exception.Message.Should().Contain("YouTube link cannot be empty");
      exception.InvalidLink.Should().Be(link);
    }

    [Fact]
    public async Task ValidateLink_WhenLinkIsWhitespace_ShouldThrowYoutubeValidationException()
    {
      // Arrange
      var link = "   ";

      // Act & Assert
      var exception = await Assert.ThrowsAsync<YoutubeValidationException>(() =>
        _sut.ValidateLink(link));

      // Whitespace is not considered empty by string.IsNullOrEmpty, so it's treated as invalid format
      exception.Message.Should().Contain("Invalid YouTube link format");
    }

    #endregion

    #region ValidateLink - Invalid Format Tests

    [Theory]
    [InlineData("https://www.google.com")]
    [InlineData("not a url")]
    [InlineData("https://youtube.com")]
    [InlineData("https://www.youtube.com/watch")]
    [InlineData("https://vimeo.com/123456789")]
    public async Task ValidateLink_WhenInvalidFormat_ShouldThrowYoutubeValidationException(string invalidLink)
    {
      // Act & Assert
      var exception = await Assert.ThrowsAsync<YoutubeValidationException>(() =>
        _sut.ValidateLink(invalidLink));

      exception.Message.Should().Contain("Invalid YouTube link format");
      exception.Message.Should().Contain("Could not extract video ID");
      exception.InvalidLink.Should().Be(invalidLink);
    }

    #endregion

    #region ValidateLink - Valid Links Tests

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/embed/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/v/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ&feature=share")]
    public async Task ValidateLink_WhenValidFormatAndAccessible_ShouldNotThrowException(string validLink)
    {
      // Arrange
      SetupSuccessfulHttpResponse();

      // Act
      var act = async () => await _sut.ValidateLink(validLink);

      // Assert
      await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ValidateLink_WhenValidLink_ShouldCallYoutubeThumbnailEndpoint()
    {
      // Arrange
      var link = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
      var videoId = "dQw4w9WgXcQ";
      SetupSuccessfulHttpResponse();

      // Act
      await _sut.ValidateLink(link);

      // Assert
      _httpMessageHandlerMock.Protected().Verify(
        "SendAsync",
        Times.Once(),
        ItExpr.Is<HttpRequestMessage>(req =>
          req.Method == HttpMethod.Get &&
          req.RequestUri!.ToString() == $"https://img.youtube.com/vi/{videoId}/0.jpg"),
        ItExpr.IsAny<CancellationToken>()
      );
    }

    #endregion

    #region ValidateLink - Video Accessibility Tests

    [Fact]
    public async Task ValidateLink_WhenVideoNotAccessible_ShouldThrowYoutubeValidationException()
    {
      // Arrange
      var link = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
      SetupHttpResponse(HttpStatusCode.NotFound);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<YoutubeValidationException>(() =>
        _sut.ValidateLink(link));

      exception.Message.Should().Contain("appears to be private, deleted, or not available");
      exception.InvalidLink.Should().Be(link);
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.Gone)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task ValidateLink_WhenVideoReturnsErrorStatusCode_ShouldThrowYoutubeValidationException(HttpStatusCode statusCode)
    {
      // Arrange
      var link = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
      SetupHttpResponse(statusCode);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<YoutubeValidationException>(() =>
        _sut.ValidateLink(link));

      exception.Message.Should().Contain("appears to be private, deleted, or not available");
    }

    [Fact]
    public async Task ValidateLink_WhenHttpRequestFails_ShouldThrowYoutubeValidationException()
    {
      // Arrange
      var link = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
      SetupHttpRequestException();

      // Act & Assert
      var exception = await Assert.ThrowsAsync<YoutubeValidationException>(() =>
        _sut.ValidateLink(link));

      exception.Message.Should().Contain("Could not verify if the YouTube video is accessible");
      // The HttpRequestException is caught and wrapped, but the validator doesn't pass it as InnerException
      // in the HttpRequestException catch block
    }

    #endregion

    #region ValidateLink - Unexpected Error Tests

    [Fact]
    public async Task ValidateLink_WhenUnexpectedExceptionOccurs_ShouldThrowYoutubeValidationException()
    {
      // Arrange
      var link = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
      _httpMessageHandlerMock.Protected()
        .Setup<Task<HttpResponseMessage>>(
          "SendAsync",
          ItExpr.IsAny<HttpRequestMessage>(),
          ItExpr.IsAny<CancellationToken>())
        .ThrowsAsync(new InvalidOperationException("Unexpected error"));

      // Act & Assert
      var exception = await Assert.ThrowsAsync<YoutubeValidationException>(() =>
        _sut.ValidateLink(link));

      exception.Message.Should().Contain("An unexpected error occurred while validating the YouTube link");
      exception.InnerException.Should().BeOfType<InvalidOperationException>();
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task ValidateLink_WhenLinkIsEmpty_ShouldLogWarning()
    {
      // Arrange
      var link = "";

      // Act
      try
      {
        await _sut.ValidateLink(link);
      }
      catch (YoutubeValidationException)
      {
        // Expected
      }

      // Assert
      _loggerMock.Verify(
        x => x.Log(
          LogLevel.Warning,
          It.IsAny<EventId>(),
          It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Empty YouTube link received")),
          It.IsAny<Exception>(),
          It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
    }

    [Fact]
    public async Task ValidateLink_WhenInvalidFormat_ShouldLogWarning()
    {
      // Arrange
      var link = "https://www.google.com";

      // Act
      try
      {
        await _sut.ValidateLink(link);
      }
      catch (YoutubeValidationException)
      {
        // Expected
      }

      // Assert
      _loggerMock.Verify(
        x => x.Log(
          LogLevel.Warning,
          It.IsAny<EventId>(),
          It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid YouTube link format")),
          It.IsAny<Exception>(),
          It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
    }

    [Fact]
    public async Task ValidateLink_WhenVideoNotAccessible_ShouldLogWarning()
    {
      // Arrange
      var link = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
      SetupHttpResponse(HttpStatusCode.NotFound);

      // Act
      try
      {
        await _sut.ValidateLink(link);
      }
      catch (YoutubeValidationException)
      {
        // Expected
      }

      // Assert
      _loggerMock.Verify(
        x => x.Log(
          LogLevel.Warning,
          It.IsAny<EventId>(),
          It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("YouTube video not accessible")),
          It.IsAny<Exception>(),
          It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
    }

    [Fact]
    public async Task ValidateLink_WhenHttpRequestFails_ShouldLogWarning()
    {
      // Arrange
      var link = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
      SetupHttpRequestException();

      // Act
      try
      {
        await _sut.ValidateLink(link);
      }
      catch (YoutubeValidationException)
      {
        // Expected
      }

      // Assert
      _loggerMock.Verify(
        x => x.Log(
          LogLevel.Warning,
          It.IsAny<EventId>(),
          It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to validate YouTube video accessibility")),
          It.IsAny<Exception>(),
          It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
    }

    [Fact]
    public async Task ValidateLink_WhenUnexpectedException_ShouldLogError()
    {
      // Arrange
      var link = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
      _httpMessageHandlerMock.Protected()
        .Setup<Task<HttpResponseMessage>>(
          "SendAsync",
          ItExpr.IsAny<HttpRequestMessage>(),
          ItExpr.IsAny<CancellationToken>())
        .ThrowsAsync(new InvalidOperationException("Unexpected error"));

      // Act
      try
      {
        await _sut.ValidateLink(link);
      }
      catch (YoutubeValidationException)
      {
        // Expected
      }

      // Assert
      _loggerMock.Verify(
        x => x.Log(
          LogLevel.Error,
          It.IsAny<EventId>(),
          It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unexpected error during YouTube link validation")),
          It.IsAny<Exception>(),
          It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
    }

    #endregion

    #region Video ID Extraction Tests

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=abc123DEF45", "abc123DEF45")]
    [InlineData("https://youtu.be/xyz987UVW32", "xyz987UVW32")]
    [InlineData("https://www.youtube.com/embed/test1234567", "test1234567")]
    [InlineData("https://www.youtube.com/v/sample12345", "sample12345")]
    public async Task ValidateLink_ShouldExtractCorrectVideoId(string link, string expectedVideoId)
    {
      // Arrange
      SetupSuccessfulHttpResponse();

      // Act
      await _sut.ValidateLink(link);

      // Assert
      _httpMessageHandlerMock.Protected().Verify(
        "SendAsync",
        Times.Once(),
        ItExpr.Is<HttpRequestMessage>(req =>
          req.RequestUri!.ToString().Contains(expectedVideoId)),
        ItExpr.IsAny<CancellationToken>()
      );
    }

    #endregion

    #region Helper Methods

    private void SetupSuccessfulHttpResponse()
    {
      SetupHttpResponse(HttpStatusCode.OK);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode)
    {
      _httpMessageHandlerMock.Protected()
        .Setup<Task<HttpResponseMessage>>(
          "SendAsync",
          ItExpr.IsAny<HttpRequestMessage>(),
          ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
          StatusCode = statusCode,
          Content = new StringContent(""),
        });
    }

    private void SetupHttpRequestException()
    {
      _httpMessageHandlerMock.Protected()
        .Setup<Task<HttpResponseMessage>>(
          "SendAsync",
          ItExpr.IsAny<HttpRequestMessage>(),
          ItExpr.IsAny<CancellationToken>())
        .ThrowsAsync(new HttpRequestException("Network error"));
    }

    #endregion
  }
}