using System;
using System.ComponentModel.DataAnnotations;

namespace back.Models;

public class User
{
  public int Id { get; set; }

  [Required]
  public string Name { get; set; } = string.Empty;

  public int Score { get; set; } = 0;

  public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

  public bool IsActive { get; set; } = true;

  public enum Role
  {
    Member,
    Admin,
    Owner
  }

  public Role UserRole { get; set; } = Role.Member;
}
