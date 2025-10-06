namespace back.Models;

public class Team
{
  public int Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public bool IsPrivate { get; set; }

  public List<Song> Songs { get; set; } = new List<Song>();
  public List<User> Users { get; set; } = new List<User>();

  public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
