using System.ComponentModel.DataAnnotations;

namespace back.Models;

public record Song : IComparable<Song>
{
  public int Id { get; set; }

  [Required]
  public string Link { get; init; } = string.Empty;

  public string Title { get; init; } = string.Empty;

  public string Artist { get; init; } = string.Empty;

  public int Rating { get; set; } = 0;

  public int AddedByUserId { get; init; }

  public string AddedByUserName { get; init; } = string.Empty;

  public DateTime AddedAt { get; init; } = DateTime.UtcNow;

  public int Index { get; set; } = 0;

  public string ThumbnailUrl { get; init; } = string.Empty;

  public int DurationSeconds { get; init; } = 0;

  public int CompareTo(Song? other)
  {
    if (other == null) return 1;
    return Rating.CompareTo(other.Rating);
  }
}
