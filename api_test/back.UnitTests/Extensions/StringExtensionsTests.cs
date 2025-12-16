using FluentAssertions;
using back.Extensions;

namespace back.Tests.Extensions
{
    public class StringExtensionsTests
    {
        #region TruncateWithEllipsis Tests

        [Fact]
        public void TruncateWithEllipsis_WhenStringIsShorterThanMaxLength_ShouldReturnOriginalString()
        {
            // Arrange
            string text = "Hello";
            int maxLength = 10;

            // Act
            var result = text.TruncateWithEllipsis(maxLength);

            // Assert
            result.Should().Be("Hello");
        }

        [Fact]
        public void TruncateWithEllipsis_WhenStringIsEqualToMaxLength_ShouldReturnOriginalString()
        {
            // Arrange
            string text = "Hello";
            int maxLength = 5;

            // Act
            var result = text.TruncateWithEllipsis(maxLength);

            // Assert
            result.Should().Be("Hello");
        }

        [Fact]
        public void TruncateWithEllipsis_WhenStringIsLongerThanMaxLength_ShouldTruncateAndAddEllipsis()
        {
            // Arrange
            string text = "Hello World";
            int maxLength = 5;

            // Act
            var result = text.TruncateWithEllipsis(maxLength);

            // Assert
            result.Should().Be("Hello...");
        }

        [Fact]
        public void TruncateWithEllipsis_WhenStringIsEmpty_ShouldReturnEmpty()
        {
            // Arrange
            string text = "";
            int maxLength = 5;

            // Act
            var result = text.TruncateWithEllipsis(maxLength);

            // Assert
            result.Should().Be("");
        }

        [Fact]
        public void TruncateWithEllipsis_WhenStringIsNull_ShouldReturnNull()
        {
            // Arrange
            string text = null!;
            int maxLength = 5;

            // Act
            var result = text.TruncateWithEllipsis(maxLength);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void TruncateWithEllipsis_WhenMaxLengthIsZero_ShouldTruncateToEmpty()
        {
            // Arrange
            string text = "Hello";
            int maxLength = 0;

            // Act
            var result = text.TruncateWithEllipsis(maxLength);

            // Assert
            result.Should().Be("...");
        }

        [Fact]
        public void TruncateWithEllipsis_WhenMaxLengthIsOne_ShouldReturnFirstCharacterAndEllipsis()
        {
            // Arrange
            string text = "Hello World";
            int maxLength = 1;

            // Act
            var result = text.TruncateWithEllipsis(maxLength);

            // Assert
            result.Should().Be("H...");
        }

        [Fact]
        public void TruncateWithEllipsis_WithLongString_ShouldTruncateCorrectly()
        {
            // Arrange
            string text = "The quick brown fox jumps over the lazy dog";
            int maxLength = 10;

            // Act
            var result = text.TruncateWithEllipsis(maxLength);

            // Assert
            result.Should().Be("The quick ...");
        }

        [Fact]
        public void TruncateWithEllipsis_WithSpecialCharacters_ShouldTruncateCorrectly()
        {
            // Arrange
            string text = "Hello@World#123";
            int maxLength = 5;

            // Act
            var result = text.TruncateWithEllipsis(maxLength);

            // Assert
            result.Should().Be("Hello...");
        }

        #endregion
    }
}
