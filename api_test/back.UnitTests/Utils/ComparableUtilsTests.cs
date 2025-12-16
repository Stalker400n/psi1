using FluentAssertions;
using back.Utils;

namespace back.Tests.Utils
{
    // Wrapper class to test generics with reference types
    public class ComparableInt : IComparable<ComparableInt>
    {
        public int Value { get; set; }

        public ComparableInt(int value)
        {
            Value = value;
        }

        public int CompareTo(ComparableInt? other)
        {
            if (other == null) return 1;
            return Value.CompareTo(other.Value);
        }

        public override bool Equals(object? obj)
        {
            return obj is ComparableInt cInt && cInt.Value == Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }

    public class ComparableUtilsTests
    {
        private readonly ComparableUtils _sut;

        public ComparableUtilsTests()
        {
            _sut = new ComparableUtils();
        }

        #region SortByComparable Tests

        [Fact]
        public void SortByComparable_WithComparableIntList_ShouldSortAscending()
        {
            // Arrange
            var items = new List<ComparableInt>
      {
        new ComparableInt(5),
        new ComparableInt(2),
        new ComparableInt(8),
        new ComparableInt(1),
        new ComparableInt(9),
        new ComparableInt(3)
      };

            // Act
            var result = _sut.SortByComparable(items);

            // Assert
            result.Should().HaveCount(6);
            result[0].Value.Should().Be(1);
            result[1].Value.Should().Be(2);
            result[2].Value.Should().Be(3);
            result[3].Value.Should().Be(5);
            result[4].Value.Should().Be(8);
            result[5].Value.Should().Be(9);
        }

        [Fact]
        public void SortByComparable_WithStringList_ShouldSortAlphabetically()
        {
            // Arrange
            var items = new List<string> { "zebra", "apple", "mango", "banana" };

            // Act
            var result = _sut.SortByComparable(items);

            // Assert
            result.Should().BeInAscendingOrder();
            result.Should().Equal(new List<string> { "apple", "banana", "mango", "zebra" });
        }

        [Fact]
        public void SortByComparable_WithEmptyList_ShouldReturnEmptyList()
        {
            // Arrange
            var items = new List<ComparableInt>();

            // Act
            var result = _sut.SortByComparable(items);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void SortByComparable_WithSingleElement_ShouldReturnSameList()
        {
            // Arrange
            var items = new List<ComparableInt> { new ComparableInt(42) };

            // Act
            var result = _sut.SortByComparable(items);

            // Assert
            result.Should().ContainSingle().Which.Value.Should().Be(42);
        }

        [Fact]
        public void SortByComparable_WithDuplicates_ShouldSortCorrectly()
        {
            // Arrange
            var items = new List<ComparableInt>
      {
        new ComparableInt(3),
        new ComparableInt(1),
        new ComparableInt(3),
        new ComparableInt(2),
        new ComparableInt(1),
        new ComparableInt(3)
      };

            // Act
            var result = _sut.SortByComparable(items);

            // Assert
            result.Should().HaveCount(6);
            result[0].Value.Should().Be(1);
            result[1].Value.Should().Be(1);
            result[2].Value.Should().Be(2);
            result[3].Value.Should().Be(3);
            result[4].Value.Should().Be(3);
            result[5].Value.Should().Be(3);
        }

        [Fact]
        public void SortByComparable_WithNegativeNumbers_ShouldSortCorrectly()
        {
            // Arrange
            var items = new List<ComparableInt>
      {
        new ComparableInt(-5),
        new ComparableInt(3),
        new ComparableInt(-1),
        new ComparableInt(0),
        new ComparableInt(2),
        new ComparableInt(-10)
      };

            // Act
            var result = _sut.SortByComparable(items);

            // Assert
            result.Should().HaveCount(6);
            result[0].Value.Should().Be(-10);
            result[1].Value.Should().Be(-5);
            result[2].Value.Should().Be(-1);
            result[3].Value.Should().Be(0);
            result[4].Value.Should().Be(2);
            result[5].Value.Should().Be(3);
        }

        #endregion

        #region FindMinimum Tests

        [Fact]
        public void FindMinimum_WithComparableIntList_ShouldReturnMinimumValue()
        {
            // Arrange
            var items = new List<ComparableInt>
      {
        new ComparableInt(5),
        new ComparableInt(2),
        new ComparableInt(8),
        new ComparableInt(1),
        new ComparableInt(9),
        new ComparableInt(3)
      };

            // Act
            var result = _sut.FindMinimum(items);

            // Assert
            result.Should().NotBeNull();
            result!.Value.Should().Be(1);
        }

        [Fact]
        public void FindMinimum_WithStringList_ShouldReturnMinimumString()
        {
            // Arrange
            var items = new List<string> { "zebra", "apple", "mango", "banana" };

            // Act
            var result = _sut.FindMinimum(items);

            // Assert
            result.Should().Be("apple");
        }

        [Fact]
        public void FindMinimum_WithEmptyList_ShouldReturnNull()
        {
            // Arrange
            var items = new List<ComparableInt>();

            // Act
            var result = _sut.FindMinimum(items);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void FindMinimum_WithSingleElement_ShouldReturnThatElement()
        {
            // Arrange
            var items = new List<ComparableInt> { new ComparableInt(42) };

            // Act
            var result = _sut.FindMinimum(items);

            // Assert
            result.Should().NotBeNull();
            result!.Value.Should().Be(42);
        }

        [Fact]
        public void FindMinimum_WithDuplicateMinimums_ShouldReturnFirstMinimum()
        {
            // Arrange
            var items = new List<ComparableInt>
      {
        new ComparableInt(5),
        new ComparableInt(1),
        new ComparableInt(8),
        new ComparableInt(1),
        new ComparableInt(9),
        new ComparableInt(3)
      };

            // Act
            var result = _sut.FindMinimum(items);

            // Assert
            result.Should().NotBeNull();
            result!.Value.Should().Be(1);
        }

        [Fact]
        public void FindMinimum_WithNegativeNumbers_ShouldReturnSmallestNegative()
        {
            // Arrange
            var items = new List<ComparableInt>
      {
        new ComparableInt(-5),
        new ComparableInt(3),
        new ComparableInt(-1),
        new ComparableInt(0),
        new ComparableInt(2),
        new ComparableInt(-10)
      };

            // Act
            var result = _sut.FindMinimum(items);

            // Assert
            result.Should().NotBeNull();
            result!.Value.Should().Be(-10);
        }

        [Fact]
        public void FindMinimum_WithAllNegativeNumbers_ShouldReturnSmallestNegative()
        {
            // Arrange
            var items = new List<ComparableInt>
      {
        new ComparableInt(-5),
        new ComparableInt(-3),
        new ComparableInt(-1),
        new ComparableInt(-8),
        new ComparableInt(-2)
      };

            // Act
            var result = _sut.FindMinimum(items);

            // Assert
            result.Should().NotBeNull();
            result!.Value.Should().Be(-8);
        }

        #endregion
    }
}
