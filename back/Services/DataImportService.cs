using back.Data.Repositories;
using back.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace back.Services
{
  public class DataImportService : IDataImportService
  {
    private readonly ITeamsRepository _teamsRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly ISongsRepository _songsRepository;
    private readonly IChatsRepository _chatsRepository;
    public DataImportService(ITeamsRepository teamsRepository,
      IUsersRepository usersRepository,
      ISongsRepository songsRepository,
      IChatsRepository chatsRepository)
    {
      if (teamsRepository == null) throw new ArgumentNullException(nameof(teamsRepository));
      if (usersRepository == null) throw new ArgumentNullException(nameof(songsRepository));
      if (songsRepository == null) throw new ArgumentNullException(nameof(songsRepository));
      if (chatsRepository == null) throw new ArgumentNullException(nameof(chatsRepository));

      _teamsRepository = teamsRepository;
      _usersRepository = usersRepository;
      _songsRepository = songsRepository;
      _chatsRepository = chatsRepository;
    }
    public async Task ImportData(string filePath)
    {
      if (!File.Exists(filePath))
      {
        throw new FileNotFoundException($"The file at path {filePath} was not found.");
      }
      await using FileStream stream = File.OpenRead(filePath);
      var importData = JsonSerializer.Deserialize<List<Team>>(stream, new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
      });
      if (importData == null || importData.Count == 0)
      {
        throw new InvalidOperationException("No data found to import.");
      }
      if (importData != null)
      {
        foreach (var team in importData)
        {
          var users = team.Users?.ToList();
          var songs = team.Songs?.ToList();
          var messages = team.Messages?.ToList();
          team.Users = new List<User>();
          team.Songs = new List<Song>();
          team.Messages = new List<ChatMessage>();
          var createdTeam = await _teamsRepository.CreateAsync(team);
          if (users != null)
          {
            foreach (var user in users)
            {
              user.Id = 0;
              await _usersRepository.CreateUserAsync(createdTeam.Id, user);
            }
          }
          if (songs != null)
          {
            foreach (var song in songs)
            {
              song.Id = 0;
              await _songsRepository.AddSongAsync(createdTeam.Id, song);
            }
          }
          if (messages != null)
          {
            foreach (var message in messages)
            {
              message.Id = 0;
              await _chatsRepository.AddMessageAsync(createdTeam.Id, message);
            }
          }
        }
      }
    }
  }
}