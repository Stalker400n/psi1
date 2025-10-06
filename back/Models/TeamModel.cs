using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace back.Models;

public class Team
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public bool IsPrivate { get; set; }
    
    public string InviteCode { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int CreatedByUserId { get; set; }
    
    public List<Song> Songs { get; set; } = new List<Song>();
    
    public List<User> Users { get; set; } = new List<User>();
    
    public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
