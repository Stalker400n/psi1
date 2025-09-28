namespace back.Models;

public class Team
{
  public int Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public bool IsPrivate { get; set; }
}
