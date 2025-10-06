using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using back.Models;
using System.Collections.Generic;

namespace back
{
  public class TestApi
  {
    private static readonly HttpClient client = new HttpClient();
    private static readonly string baseUrl = "http://localhost:5220/";
    private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static async Task Main(string[] args)
    {
      Console.WriteLine("Starting API Test...");

      try
      {
        // Create a team
        var teamId = await CreateTeam();
        Console.WriteLine($"Created team with ID: {teamId}");

        // Add users to the team
        var user1Id = await AddUser(teamId, "John Doe", "john@example.com");
        var user2Id = await AddUser(teamId, "Jane Smith", "jane@example.com");
        Console.WriteLine($"Added users with IDs: {user1Id}, {user2Id}");

        // Add songs to the team
        var song1Id = await AddSong(teamId, "https://example.com/song1", "Song One", "Artist One", user1Id, "John Doe");
        var song2Id = await AddSong(teamId, "https://example.com/song2", "Song Two", "Artist Two", user2Id, "Jane Smith");
        Console.WriteLine($"Added songs with IDs: {song1Id}, {song2Id}");

        // Add chat messages to the team
        var message1Id = await AddChatMessage(teamId, "John Doe", "Hello everyone!");
        var message2Id = await AddChatMessage(teamId, "Jane Smith", "Hi John, how are you?");
        Console.WriteLine($"Added chat messages with IDs: {message1Id}, {message2Id}");

        // Get team details
        var team = await GetTeam(teamId);
        Console.WriteLine($"Retrieved team: {team.Name}, Users: {team.Users.Count}, Songs: {team.Songs.Count}, Messages: {team.Messages.Count}");

        Console.WriteLine("API Test completed successfully!");
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error during API test: {ex.Message}");
        Console.WriteLine(ex.StackTrace);
      }
    }

    private static async Task<int> CreateTeam()
    {
      var team = new Team
      {
        Name = "Test Team",
        IsPrivate = false,
        InviteCode = "TEST123",
        CreatedByUserId = 1
      };

      var response = await client.PostAsJsonAsync($"{baseUrl}/teams", team);
      response.EnsureSuccessStatusCode();

      var content = await response.Content.ReadAsStringAsync();
      var createdTeam = JsonSerializer.Deserialize<Team>(content, jsonOptions);

      return createdTeam.Id;
    }

    private static async Task<int> AddUser(int teamId, string name, string email)
    {
      var user = new User
      {
        Name = name,
        Email = email,
        Score = 0,
        IsActive = true
      };

      var response = await client.PostAsJsonAsync($"{baseUrl}/teams/{teamId}/users", user);
      response.EnsureSuccessStatusCode();

      var content = await response.Content.ReadAsStringAsync();
      var createdUser = JsonSerializer.Deserialize<User>(content, jsonOptions);

      return createdUser.Id;
    }

    private static async Task<int> AddSong(int teamId, string link, string title, string artist, int userId, string userName)
    {
      var song = new Song
      {
        Link = link,
        Title = title,
        Artist = artist,
        Rating = 0,
        AddedByUserId = userId,
        AddedByUserName = userName
      };

      var response = await client.PostAsJsonAsync($"{baseUrl}/teams/{teamId}/songs", song);
      response.EnsureSuccessStatusCode();

      var content = await response.Content.ReadAsStringAsync();
      var createdSong = JsonSerializer.Deserialize<Song>(content, jsonOptions);

      return createdSong.Id;
    }

    private static async Task<int> AddChatMessage(int teamId, string userName, string text)
    {
      var message = new ChatMessage
      {
        UserName = userName,
        Text = text
      };

      var response = await client.PostAsJsonAsync($"{baseUrl}/teams/{teamId}/chats", message);
      response.EnsureSuccessStatusCode();

      var content = await response.Content.ReadAsStringAsync();
      var createdMessage = JsonSerializer.Deserialize<ChatMessage>(content, jsonOptions);

      return createdMessage.Id;
    }

    private static async Task<Team> GetTeam(int teamId)
    {
      var response = await client.GetAsync($"{baseUrl}/teams/{teamId}");
      response.EnsureSuccessStatusCode();

      var content = await response.Content.ReadAsStringAsync();
      var team = JsonSerializer.Deserialize<Team>(content, jsonOptions);

      return team;
    }
  }
}
