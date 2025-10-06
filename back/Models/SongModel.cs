using System;
using System.ComponentModel.DataAnnotations;

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
}
