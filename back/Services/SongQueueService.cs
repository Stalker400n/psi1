using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back.Models;
using back.Data.Repositories;

namespace back.Services
{
  public class SongQueueService : ISongQueueService
  {
    private readonly Dictionary<int, SongQueue> _queues = new Dictionary<int, SongQueue>();
    private readonly ITeamsRepository _teamsRepository;
    private readonly ISongsRepository _songsRepository;

    public SongQueueService(ITeamsRepository teamsRepository, ISongsRepository songsRepository)
    {
      _teamsRepository = teamsRepository;
      _songsRepository = songsRepository;
    }

    public async Task InitializeQueueAsync(int teamId)
    {
      var team = await _teamsRepository.GetByIdAsync(teamId);
      if (team == null) return;

      var allSongs = await _songsRepository.GetSongsAsync(teamId);
      if (allSongs == null) return;

      var queue = GetOrCreateQueue(teamId);
      queue.Clear();

      var upcomingSongs = allSongs
        .Where(s => s.Index >= team.CurrentSongIndex)
        .OrderBy(s => s.Index)
        .ToList();

      foreach (var song in upcomingSongs)
      {
        queue.Enqueue(song);
      }
    }

    public async Task<List<Song>> GetQueueAsync(int teamId)
    {
      var queue = GetOrCreateQueue(teamId);
      
      if (queue.Count == 0)
      {
        await InitializeQueueAsync(teamId);
      }

      return queue.ToList();
    }

    public async Task<Song?> GetCurrentSongAsync(int teamId)
    {
      var team = await _teamsRepository.GetByIdAsync(teamId);
      if (team == null) return null;

      var allSongs = await _songsRepository.GetSongsAsync(teamId);
      if (allSongs == null) return null;

      return allSongs.FirstOrDefault(s => s.Index == team.CurrentSongIndex);
    }

    public async Task<Song?> AdvanceToNextSongAsync(int teamId)
    {
      var team = await _teamsRepository.GetByIdAsync(teamId);
      if (team == null) return null;

      var allSongs = await _songsRepository.GetSongsAsync(teamId);
      if (allSongs == null) return null;

      var songsList = allSongs.ToList();
      var maxIndex = songsList.Any() ? songsList.Max(s => s.Index) : -1;

      if (team.CurrentSongIndex >= maxIndex) return null;

      team.CurrentSongIndex++;
      await _teamsRepository.UpdateAsync(teamId, team);

      await InitializeQueueAsync(teamId);

      return songsList.FirstOrDefault(s => s.Index == team.CurrentSongIndex);
    }

    public async Task<Song?> GoToPreviousSongAsync(int teamId)
    {
      var team = await _teamsRepository.GetByIdAsync(teamId);
      if (team == null) return null;

      if (team.CurrentSongIndex <= 0) return null;

      team.CurrentSongIndex--;
      await _teamsRepository.UpdateAsync(teamId, team);

      await InitializeQueueAsync(teamId);

      var allSongs = await _songsRepository.GetSongsAsync(teamId);
      return allSongs?.FirstOrDefault(s => s.Index == team.CurrentSongIndex);
    }

    public async Task<Song?> JumpToSongAsync(int teamId, int index)
    {
      var team = await _teamsRepository.GetByIdAsync(teamId);
      if (team == null) return null;

      var allSongs = await _songsRepository.GetSongsAsync(teamId);
      if (allSongs == null) return null;

      var targetSong = allSongs.FirstOrDefault(s => s.Index == index);
      if (targetSong == null) return null;

      team.CurrentSongIndex = index;
      await _teamsRepository.UpdateAsync(teamId, team);

      await InitializeQueueAsync(teamId);

      return targetSong;
    }

    public async Task RefreshQueueAsync(int teamId)
    {
      await InitializeQueueAsync(teamId);
    }

    private SongQueue GetOrCreateQueue(int teamId)
    {
      if (!_queues.ContainsKey(teamId))
        _queues[teamId] = new SongQueue();

      return _queues[teamId];
    }
  }
}
