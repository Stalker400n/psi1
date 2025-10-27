using back.Data.Repositories;
using back.Models;
using System.Text.Json;

namespace back.Services
{
    public class DataImportService : IDataImportService
    {
        private readonly ITeamsRepository _teamsRepository;
        private readonly IUsersRepository _usersRepository;
        private readonly ISongsRepository _songsRepository;
        private readonly IChatsRepository _chatsRepository;

        public DataImportService(
            ITeamsRepository teamsRepository,
            IUsersRepository usersRepository,
            ISongsRepository songsRepository,
            IChatsRepository chatsRepository)
        {
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

            string json = await File.ReadAllTextAsync(filePath);

            var importData = JsonSerializer.Deserialize<List<Team>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (importData == null || importData.Count == 0)
            {
                throw new InvalidOperationException("No data found to import.");
            }
            if (importData != null)
            {
                foreach (var team in importData)
                {
                    Console.WriteLine($"Team: {team.Id} - {team.Name}");
                    // Store related entities temporarily
                    var users = team.Users?.ToList();
                    var songs = team.Songs?.ToList();
                    var messages = team.Messages?.ToList();

                    // Clear navigation properties before creating team
                    team.Users = new List<User>();
                    team.Songs = new List<Song>();
                    team.Messages = new List<ChatMessage>();

                    // Create the team first
                    var createdTeam = await _teamsRepository.CreateAsync(team);

                    // Now add related entities with proper foreign keys
                    if (users != null)
                    {
                        foreach (var user in users)
                        {
                            await _usersRepository.CreateUserAsync(createdTeam.Id, user);
                            Console.WriteLine($" User: {user.Id} - {user.Name}");
                        }
                    }

                    if (songs != null)
                    {
                        foreach (var song in songs)
                        {
                            await _songsRepository.AddSongAsync(createdTeam.Id, song);
                            Console.WriteLine($" Song: {song.Id} - {song.Link}");
                        }
                    }

                    if (messages != null)
                    {
                        foreach (var msg in messages)
                        {
                            await _chatsRepository.AddMessageAsync(createdTeam.Id, msg);
                            Console.WriteLine($" Message: {msg.Id}");
                        }
                    }
                }
            }



            // foreach (var raw in importData)
            // {
            //     await _teamsRepository.CreateAsync(raw);

            //     foreach (var user in raw.Users)
            //     {
            //         await _usersRepository.CreateUserAsync(raw.Id, user);
            //     }

            //     foreach (var song in raw.Songs)
            //     {
            //         await _songsRepository.AddSongAsync(raw.Id, song);
            //     }

            //     foreach (var message in raw.Messages)
            //     {
            //         await _chatsRepository.AddMessageAsync(raw.Id, message);
            //     }
            // }

        }
    }



public class RawTeamData
{
  public int Id { get; set; }

  public string Name { get; set; } = string.Empty;

  public bool IsPrivate { get; set; }

  public string InviteCode { get; set; } = string.Empty;

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  public int CreatedByUserId { get; set; }

  public int CurrentSongIndex { get; set; } = 0;

  public List<Song> Songs { get; set; } = new ();

  public List<User> Users { get; set; } = new ();

  public List<ChatMessage> Messages { get; set; } = new ();
}
}