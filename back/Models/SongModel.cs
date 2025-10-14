using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace back.Models;

public class Song
{
  public int Id { get; set; }

  [Required]
  public string Link { get; set; } = string.Empty;

  public string Title { get; set; } = string.Empty;

  public string Artist { get; set; } = string.Empty;

  public int Rating { get; set; } = 0;

  public int AddedByUserId { get; set; }

  public string AddedByUserName { get; set; } = string.Empty;

  public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public struct SongMetadata
    {
        public string Genre { get; set; }
        public TimeSpan Duration { get; set; }

        public SongMetadata(string genre, TimeSpan duration)
        {
            Genre = genre;
            Duration = duration;
        }
    }

    [NotMapped]
    public SongMetadata Metadata { get; set; } = new SongMetadata("Unknown", TimeSpan.Zero);
}
