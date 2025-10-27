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
                    var createdTeam = await _teamsRepository.CreateAsync(team);
                }
            }
        }
    }
}