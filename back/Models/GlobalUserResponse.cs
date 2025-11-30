namespace back.Models;

public class GlobalUserResponse
{
  public int Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public bool IsNew { get; set; }
}
