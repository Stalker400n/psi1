using System.ComponentModel.DataAnnotations;

namespace back.Models;

public class GlobalUser
{
  public int Id { get; set; }

  [Required]
  [StringLength(50, MinimumLength = 2)]
  public string Name { get; set; } = string.Empty;

  [Required]
  public string DeviceFingerprint { get; set; } = string.Empty;

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;

  public string DeviceInfo { get; set; } = string.Empty;
}
