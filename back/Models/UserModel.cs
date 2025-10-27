using System;
using System.ComponentModel.DataAnnotations;
using back.Models.Enums;

namespace back.Models;

public class User
{
  public int Id { get; set; }

  [Required]
  public string Name { get; set; } = string.Empty;

  public int Score { get; set; } = 0;

  public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

  public bool IsActive { get; set; } = true;

  public Role Role { get; set; } = Role.Member;
}
