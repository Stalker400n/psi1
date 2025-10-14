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


  public struct UserInfo
  {
        public int Id { get; set; }
        public string Name { get; set; }

        public UserInfo(int id, string name)
        {
            Id = id;
            Name = name;
        }
  }

  public UserInfo AddedBy { get; set; }

  public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
