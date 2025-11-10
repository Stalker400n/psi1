using System.ComponentModel.DataAnnotations;

namespace back.Models;

public class SongRating
{
  public int Id { get; set; }

  [Required]
  public int SongId { get; set; }

  [Required]
  public int UserId { get; set; }

  [Range(0, 100)]
  public int Rating { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}