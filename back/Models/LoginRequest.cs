using System.ComponentModel.DataAnnotations;

namespace back.Models;

public class LoginRequest
{
  [Required]
  [StringLength(50, MinimumLength = 2)]
  public string Name { get; set; } = string.Empty;

  [Required]
  public string DeviceFingerprint { get; set; } = string.Empty;

  public string? DeviceInfo { get; set; }
}
