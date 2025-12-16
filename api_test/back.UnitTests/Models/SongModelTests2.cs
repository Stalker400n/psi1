using FluentAssertions;
using back.Models;

namespace back.Tests.Models
{
    public class SongModelTests
    {
        #region Constructor and Property Tests

        [Fact]
        public void Song_DefaultConstructor_CreatesWithDefaultValues()
        {
            // Arrange & Act
            var song = new Song();

            // Assert
            song.Id.Should().Be(0);
            song.Link.Should().Be(string.Empty);
            song.Title.Should().Be(string.Empty);
            song.Artist.Should().Be(string.Empty);
            song.Rating.Should().Be(0);
            song.AddedByUserId.Should().Be(0);
            song.AddedByUserName.Should().Be(string.Empty);
            song.Index.Should().Be(0);
            song.ThumbnailUrl.Should().Be(string.Empty);
            song.DurationSeconds.Should().Be(0);
            song.AddedAt.Should().NotBe(default(DateTime));
        }

        [Fact]
        public void Song_WithAllPropertiesSet_CreatesWithCorrectValues()
        {
            // Arrange
            var link = "https://youtube.com/watch?v=dQw4w9WgXcQ";
            var title = "Test Song";
            var artist = "Test Artist";
            var rating = 75;
            var userId = 123;
            var userName = "TestUser";
            var index = 2;
            var thumbnailUrl = "https://example.com/thumb.jpg";
            var durationSeconds = 180;
            var addedAt = new DateTime(2024, 1, 1, 12, 0, 0);

            // Act
            var song = new Song
            {
                Id = 1,
                Link = link,
                Title = title,
                Artist = artist,
                Rating = rating,
                AddedByUserId = userId,
                AddedByUserName = userName,
                Index = index,
                ThumbnailUrl = thumbnailUrl,
                DurationSeconds = durationSeconds,
                AddedAt = addedAt
            };

            // Assert
            song.Id.Should().Be(1);
            song.Link.Should().Be(link);
            song.Title.Should().Be(title);
            song.Artist.Should().Be(artist);
            song.Rating.Should().Be(rating);
            song.AddedByUserId.Should().Be(userId);
            song.AddedByUserName.Should().Be(userName);
            song.Index.Should().Be(index);
            song.ThumbnailUrl.Should().Be(thumbnailUrl);
            song.DurationSeconds.Should().Be(durationSeconds);
            song.AddedAt.Should().Be(addedAt);
        }

        #endregion

        #region Property Initialization Tests

        [Fact]
        public void Song_LinkProperty_IsRequired()
        {
            // Arrange & Act
            var song = new Song { Link = "https://youtube.com/watch?v=test" };

            // Assert
            song.Link.Should().Be("https://youtube.com/watch?v=test");
        }

        [Fact]
        public void Song_TitleProperty_CanBeSet()
        {
            // Arrange & Act
            var song = new Song { Title = "My Song", Link = "link" };

            // Assert
            song.Title.Should().Be("My Song");
        }

        [Fact]
        public void Song_ArtistProperty_CanBeSet()
        {
            // Arrange & Act
            var song = new Song { Artist = "My Artist", Link = "link" };

            // Assert
            song.Artist.Should().Be("My Artist");
        }

        [Fact]
        public void Song_RatingProperty_CanBeSet()
        {
            // Arrange & Act
            var song = new Song { Rating = 85, Link = "link" };

            // Assert
            song.Rating.Should().Be(85);
        }

        [Fact]
        public void Song_RatingProperty_SupportsZero()
        {
            // Arrange & Act
            var song = new Song { Rating = 0, Link = "link" };

            // Assert
            song.Rating.Should().Be(0);
        }

        [Fact]
        public void Song_RatingProperty_SupportsMaxValue()
        {
            // Arrange & Act
            var song = new Song { Rating = 100, Link = "link" };

            // Assert
            song.Rating.Should().Be(100);
        }

        [Fact]
        public void Song_RatingProperty_SupportsNegativeValue()
        {
            // Arrange & Act
            var song = new Song { Rating = -10, Link = "link" };

            // Assert
            song.Rating.Should().Be(-10);
        }

        [Fact]
        public void Song_AddedByUserIdProperty_CanBeSet()
        {
            // Arrange & Act
            var song = new Song { AddedByUserId = 456, Link = "link" };

            // Assert
            song.AddedByUserId.Should().Be(456);
        }

        [Fact]
        public void Song_AddedByUserNameProperty_CanBeSet()
        {
            // Arrange & Act
            var song = new Song { AddedByUserName = "User123", Link = "link" };

            // Assert
            song.AddedByUserName.Should().Be("User123");
        }

        [Fact]
        public void Song_IndexProperty_CanBeSet()
        {
            // Arrange & Act
            var song = new Song { Index = 5, Link = "link" };

            // Assert
            song.Index.Should().Be(5);
        }

        [Fact]
        public void Song_ThumbnailUrlProperty_CanBeSet()
        {
            // Arrange & Act
            var song = new Song { ThumbnailUrl = "https://example.com/image.jpg", Link = "link" };

            // Assert
            song.ThumbnailUrl.Should().Be("https://example.com/image.jpg");
        }

        [Fact]
        public void Song_DurationSecondsProperty_CanBeSet()
        {
            // Arrange & Act
            var song = new Song { DurationSeconds = 240, Link = "link" };

            // Assert
            song.DurationSeconds.Should().Be(240);
        }

        [Fact]
        public void Song_DurationSecondsProperty_SupportsZero()
        {
            // Arrange & Act
            var song = new Song { DurationSeconds = 0, Link = "link" };

            // Assert
            song.DurationSeconds.Should().Be(0);
        }

        [Fact]
        public void Song_AddedAtProperty_CanBeSetToCustomDateTime()
        {
            // Arrange
            var customDate = new DateTime(2025, 6, 15, 10, 30, 45);

            // Act
            var song = new Song { AddedAt = customDate, Link = "link" };

            // Assert
            song.AddedAt.Should().Be(customDate);
        }

        #endregion

        #region CompareTo Tests

        [Fact]
        public void CompareTo_WithEqualRating_ReturnsZero()
        {
            // Arrange
            var song1 = new Song { Rating = 50, Link = "link1" };
            var song2 = new Song { Rating = 50, Link = "link2" };

            // Act
            var result = song1.CompareTo(song2);

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public void CompareTo_WithHigherRating_ReturnsPositive()
        {
            // Arrange
            var song1 = new Song { Rating = 100, Link = "link1" };
            var song2 = new Song { Rating = 50, Link = "link2" };

            // Act
            var result = song1.CompareTo(song2);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public void CompareTo_WithLowerRating_ReturnsNegative()
        {
            // Arrange
            var song1 = new Song { Rating = 30, Link = "link1" };
            var song2 = new Song { Rating = 80, Link = "link2" };

            // Act
            var result = song1.CompareTo(song2);

            // Assert
            result.Should().BeLessThan(0);
        }

        [Fact]
        public void CompareTo_WithNullOther_ReturnsPositive()
        {
            // Arrange
            var song = new Song { Rating = 50, Link = "link1" };

            // Act
            var result = song.CompareTo(null);

            // Assert
            result.Should().Be(1);
        }

        [Fact]
        public void CompareTo_BothSongsWithZeroRating_ReturnsZero()
        {
            // Arrange
            var song1 = new Song { Rating = 0, Link = "link1" };
            var song2 = new Song { Rating = 0, Link = "link2" };

            // Act
            var result = song1.CompareTo(song2);

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public void CompareTo_BothSongsWithMaxRating_ReturnsZero()
        {
            // Arrange
            var song1 = new Song { Rating = 100, Link = "link1" };
            var song2 = new Song { Rating = 100, Link = "link2" };

            // Act
            var result = song1.CompareTo(song2);

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public void CompareTo_WithNegativeAndPositiveRating_ReturnsNegative()
        {
            // Arrange
            var song1 = new Song { Rating = -10, Link = "link1" };
            var song2 = new Song { Rating = 10, Link = "link2" };

            // Act
            var result = song1.CompareTo(song2);

            // Assert
            result.Should().BeLessThan(0);
        }

        #endregion

        #region Record Behavior Tests

        [Fact]
        public void Song_RecordEquality_WithIdenticalProperties_AreEqual()
        {
            // Arrange
            var fixedDate = new DateTime(2024, 1, 1, 12, 0, 0);
            var song1 = new Song { Id = 1, Link = "https://youtube.com/watch?v=test", Title = "Song", AddedAt = fixedDate };
            var song2 = new Song { Id = 1, Link = "https://youtube.com/watch?v=test", Title = "Song", AddedAt = fixedDate };

            // Act & Assert
            song1.Equals(song2).Should().BeTrue();
        }

        [Fact]
        public void Song_RecordEquality_WithDifferentId_AreNotEqual()
        {
            // Arrange
            var fixedDate = new DateTime(2024, 1, 1, 12, 0, 0);
            var song1 = new Song { Id = 1, Link = "https://youtube.com/watch?v=test", AddedAt = fixedDate };
            var song2 = new Song { Id = 2, Link = "https://youtube.com/watch?v=test", AddedAt = fixedDate };

            // Act & Assert
            song1.Equals(song2).Should().BeFalse();
        }

        [Fact]
        public void Song_RecordEquality_WithDifferentLink_AreNotEqual()
        {
            // Arrange
            var fixedDate = new DateTime(2024, 1, 1, 12, 0, 0);
            var song1 = new Song { Id = 1, Link = "https://youtube.com/watch?v=test1", AddedAt = fixedDate };
            var song2 = new Song { Id = 1, Link = "https://youtube.com/watch?v=test2", AddedAt = fixedDate };

            // Act & Assert
            song1.Equals(song2).Should().BeFalse();
        }

        [Fact]
        public void Song_RecordToString_ReturnsFormattedString()
        {
            // Arrange
            var song = new Song { Id = 1, Title = "Test Song", Link = "link" };

            // Act
            var result = song.ToString();

            // Assert
            result.Should().Contain("Song");
            result.Should().NotBeEmpty();
        }

        [Fact]
        public void Song_RecordHashCode_WithIdenticalProperties_HasSameCode()
        {
            // Arrange
            var fixedDate = new DateTime(2024, 1, 1, 12, 0, 0);
            var song1 = new Song { Id = 1, Link = "https://youtube.com/watch?v=test", Title = "Song", AddedAt = fixedDate };
            var song2 = new Song { Id = 1, Link = "https://youtube.com/watch?v=test", Title = "Song", AddedAt = fixedDate };

            // Act & Assert
            song1.GetHashCode().Should().Be(song2.GetHashCode());
        }

        #endregion

        #region Edge Cases Tests

        [Fact]
        public void Song_WithVeryLongTitle_StoresCorrectly()
        {
            // Arrange
            var longTitle = new string('A', 1000);

            // Act
            var song = new Song { Title = longTitle, Link = "link" };

            // Assert
            song.Title.Should().Be(longTitle);
            song.Title.Length.Should().Be(1000);
        }

        [Fact]
        public void Song_WithVeryLongArtist_StoresCorrectly()
        {
            // Arrange
            var longArtist = new string('B', 500);

            // Act
            var song = new Song { Artist = longArtist, Link = "link" };

            // Assert
            song.Artist.Should().Be(longArtist);
        }

        [Fact]
        public void Song_WithVeryLongLink_StoresCorrectly()
        {
            // Arrange
            var longLink = "https://youtube.com/watch?v=" + new string('x', 500);

            // Act
            var song = new Song { Link = longLink };

            // Assert
            song.Link.Should().Be(longLink);
        }

        [Fact]
        public void Song_WithSpecialCharactersInTitle_StoresCorrectly()
        {
            // Arrange
            var specialTitle = "Title !@#$%^&*() with (special) chars";

            // Act
            var song = new Song { Title = specialTitle, Link = "link" };

            // Assert
            song.Title.Should().Be(specialTitle);
        }

        [Fact]
        public void Song_WithUnicodeCharacters_StoresCorrectly()
        {
            // Arrange
            var unicodeTitle = "🎵 Song with émojis and àccents";

            // Act
            var song = new Song { Title = unicodeTitle, Link = "link" };

            // Assert
            song.Title.Should().Be(unicodeTitle);
        }

        [Fact]
        public void Song_WithLargeIntValues_StoresCorrectly()
        {
            // Arrange
            var largeId = int.MaxValue;
            var largeUserId = int.MaxValue - 1;
            var largeDuration = int.MaxValue - 2;

            // Act
            var song = new Song
            {
                Id = largeId,
                AddedByUserId = largeUserId,
                DurationSeconds = largeDuration,
                Link = "link"
            };

            // Assert
            song.Id.Should().Be(largeId);
            song.AddedByUserId.Should().Be(largeUserId);
            song.DurationSeconds.Should().Be(largeDuration);
        }

        [Fact]
        public void Song_WithMinusOneValues_StoresCorrectly()
        {
            // Arrange & Act
            var song = new Song
            {
                Id = -1,
                Rating = -1,
                Index = -1,
                DurationSeconds = -1,
                AddedByUserId = -1,
                Link = "link"
            };

            // Assert
            song.Id.Should().Be(-1);
            song.Rating.Should().Be(-1);
            song.Index.Should().Be(-1);
            song.DurationSeconds.Should().Be(-1);
            song.AddedByUserId.Should().Be(-1);
        }

        #endregion

        #region Sorting Tests

        [Fact]
        public void Song_List_CanBeSortedByRating()
        {
            // Arrange
            var songs = new List<Song>
      {
        new Song { Id = 1, Link = "link1", Rating = 30 },
        new Song { Id = 2, Link = "link2", Rating = 100 },
        new Song { Id = 3, Link = "link3", Rating = 50 }
      };

            // Act
            songs.Sort();

            // Assert
            songs[0].Rating.Should().Be(30);
            songs[1].Rating.Should().Be(50);
            songs[2].Rating.Should().Be(100);
        }

        [Fact]
        public void Song_List_WithNegativeRatings_SortsCorrectly()
        {
            // Arrange
            var songs = new List<Song>
      {
        new Song { Id = 1, Link = "link1", Rating = 10 },
        new Song { Id = 2, Link = "link2", Rating = -20 },
        new Song { Id = 3, Link = "link3", Rating = 0 }
      };

            // Act
            songs.Sort();

            // Assert
            songs[0].Rating.Should().Be(-20);
            songs[1].Rating.Should().Be(0);
            songs[2].Rating.Should().Be(10);
        }

        [Fact]
        public void Song_List_SingleElement_SortsWithoutError()
        {
            // Arrange
            var songs = new List<Song> { new Song { Id = 1, Link = "link1", Rating = 50 } };

            // Act
            songs.Sort();

            // Assert
            songs[0].Rating.Should().Be(50);
        }

        [Fact]
        public void Song_List_EmptyList_SortsWithoutError()
        {
            // Arrange
            var songs = new List<Song>();

            // Act
            songs.Sort();

            // Assert
            songs.Should().BeEmpty();
        }

        [Fact]
        public void Song_List_AllSameRating_SortsWithoutError()
        {
            // Arrange
            var songs = new List<Song>
      {
        new Song { Id = 1, Link = "link1", Rating = 50 },
        new Song { Id = 2, Link = "link2", Rating = 50 },
        new Song { Id = 3, Link = "link3", Rating = 50 }
      };

            // Act
            songs.Sort();

            // Assert
            songs.Should().HaveCount(3);
            songs.All(s => s.Rating == 50).Should().BeTrue();
        }

        #endregion

        #region Interface Implementation Tests

        [Fact]
        public void Song_ImplementsIComparable_CanBeUsedWithLinqOrderBy()
        {
            // Arrange
            var songs = new List<Song>
      {
        new Song { Id = 1, Link = "link1", Rating = 40 },
        new Song { Id = 2, Link = "link2", Rating = 20 },
        new Song { Id = 3, Link = "link3", Rating = 80 }
      };

            // Act
            var sorted = songs.OrderBy(s => s).ToList();

            // Assert
            sorted[0].Rating.Should().Be(20);
            sorted[1].Rating.Should().Be(40);
            sorted[2].Rating.Should().Be(80);
        }

        [Fact]
        public void Song_ImplementsIComparable_CanBeUsedWithLinqOrderByDescending()
        {
            // Arrange
            var songs = new List<Song>
      {
        new Song { Id = 1, Link = "link1", Rating = 40 },
        new Song { Id = 2, Link = "link2", Rating = 20 },
        new Song { Id = 3, Link = "link3", Rating = 80 }
      };

            // Act
            var sorted = songs.OrderByDescending(s => s).ToList();

            // Assert
            sorted[0].Rating.Should().Be(80);
            sorted[1].Rating.Should().Be(40);
            sorted[2].Rating.Should().Be(20);
        }

        [Fact]
        public void Song_CanBeCastToIComparable()
        {
            // Arrange
            var song = new Song { Id = 1, Link = "link", Rating = 50 };

            // Act
            var comparable = (IComparable<Song>)song;

            // Assert
            comparable.Should().NotBeNull();
        }

        #endregion

        #region Properties Get/Set Tests

        [Fact]
        public void Song_IdProperty_IsReadWritable()
        {
            // Arrange
            var song = new Song { Link = "link" };

            // Act
            song.Id = 42;
            var result = song.Id;

            // Assert
            result.Should().Be(42);
        }

        [Fact]
        public void Song_RatingProperty_IsReadWritable()
        {
            // Arrange
            var song = new Song { Rating = 0, Link = "link" };

            // Act
            song.Rating = 88;
            var result = song.Rating;

            // Assert
            result.Should().Be(88);
        }

        [Fact]
        public void Song_IndexProperty_IsReadWritable()
        {
            // Arrange
            var song = new Song { Index = 0, Link = "link" };

            // Act
            song.Index = 10;
            var result = song.Index;

            // Assert
            result.Should().Be(10);
        }

        [Fact]
        public void Song_ComplexSongWithAllProperties_StoresCorrectly()
        {
            // Arrange & Act
            var song = new Song
            {
                Id = 99,
                Link = "https://youtube.com/watch?v=abc123",
                Title = "Amazing Song",
                Artist = "Great Artist",
                Rating = 95,
                AddedByUserId = 777,
                AddedByUserName = "SuperUser",
                Index = 5,
                ThumbnailUrl = "https://example.com/img.jpg",
                DurationSeconds = 215,
                AddedAt = new DateTime(2025, 1, 15, 14, 30, 0)
            };

            // Assert - verify all properties
            song.Id.Should().Be(99);
            song.Link.Should().Be("https://youtube.com/watch?v=abc123");
            song.Title.Should().Be("Amazing Song");
            song.Artist.Should().Be("Great Artist");
            song.Rating.Should().Be(95);
            song.AddedByUserId.Should().Be(777);
            song.AddedByUserName.Should().Be("SuperUser");
            song.Index.Should().Be(5);
            song.ThumbnailUrl.Should().Be("https://example.com/img.jpg");
            song.DurationSeconds.Should().Be(215);
            song.AddedAt.Should().Be(new DateTime(2025, 1, 15, 14, 30, 0));
        }

        [Fact]
        public void Song_CompareToWithComplexProperties_OnlyComparesRating()
        {
            // Arrange
            var song1 = new Song
            {
                Id = 1,
                Link = "link1",
                Title = "Title1",
                Artist = "Artist1",
                Rating = 50,
                AddedByUserName = "User1"
            };
            var song2 = new Song
            {
                Id = 999,
                Link = "link999",
                Title = "Title999",
                Artist = "Artist999",
                Rating = 50,
                AddedByUserName = "User999"
            };

            // Act
            var result = song1.CompareTo(song2);

            // Assert - Should be equal (0) because ratings are the same
            result.Should().Be(0);
        }

        #endregion
    }
}
