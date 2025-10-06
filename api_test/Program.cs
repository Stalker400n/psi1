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
    private static readonly string baseUrl = "http://localhost:5220";
    private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static async Task Main(string[] args)
    {
      Console.WriteLine("Starting API Test...");

      try
      {
        var teamId = await CreateTeam();
        Console.WriteLine($"Created team with ID: {teamId}");

        var user1Id = await AddUser(teamId, "John Doe");
        var user2Id = await AddUser(teamId, "Jane Smith");
        Console.WriteLine($"Added users with IDs: {user1Id}, {user2Id}");

        var song1Id = await AddSong(teamId, "https://example.com/song1", "Song One", "Artist One", user1Id, "John Doe");
        var song2Id = await AddSong(teamId, "https://example.com/song2", "Song Two", "Artist Two", user2Id, "Jane Smith");
        Console.WriteLine($"Added songs with IDs: {song1Id}, {song2Id}");

        var message1Id = await AddChatMessage(teamId, "John Doe", "Hello everyone!");
        var message2Id = await AddChatMessage(teamId, "Jane Smith", "Hi John, how are you?");
        Console.WriteLine($"Added chat messages with IDs: {message1Id}, {message2Id}");

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

      Console.WriteLine($"Sending POST request to: {baseUrl}/teams");
      Console.WriteLine($"Team data: {JsonSerializer.Serialize(team, jsonOptions)}");

      try
      {
        var response = await client.PostAsJsonAsync($"{baseUrl}/teams", team);

        Console.WriteLine($"Response status: {response.StatusCode}");
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Response content: {responseContent}");

        response.EnsureSuccessStatusCode();

        var createdTeam = JsonSerializer.Deserialize<Team>(responseContent, jsonOptions);

        if (createdTeam == null)
        {
          throw new Exception("Failed to deserialize team from response");
        }

        return createdTeam.Id;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error creating team: {ex.Message}");
        throw;
      }

    }

    private static async Task<int> AddUser(int teamId, string name)
    {
      var user = new User
      {
        Name = name,
        Score = 0,
        IsActive = true
      };

      Console.WriteLine($"Sending POST request to: {baseUrl}/teams/{teamId}/users");
      Console.WriteLine($"User data: {JsonSerializer.Serialize(user, jsonOptions)}");

      try
      {
        var response = await client.PostAsJsonAsync($"{baseUrl}/teams/{teamId}/users", user);

        Console.WriteLine($"Response status: {response.StatusCode}");
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Response content: {responseContent}");

        response.EnsureSuccessStatusCode();

        var createdUser = JsonSerializer.Deserialize<User>(responseContent, jsonOptions);

        if (createdUser == null)
        {
          throw new Exception("Failed to deserialize user from response");
        }

        return createdUser.Id;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error adding user: {ex.Message}");
        throw;
      }
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
