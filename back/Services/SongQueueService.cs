using System.Collections.Generic;
using System.Linq;
using back.Models;

namespace back.Services
{
  public class SongQueueService
  {
    private readonly Dictionary<int, SongQueue> _queues = new Dictionary<int, SongQueue>();

    public void InitializeQueue(int teamId, IEnumerable<Song> allSongs, int currentSongIndex)
    {
      var queue = GetQueue(teamId);
      queue.Clear();

      var upcomingSongs = allSongs
        .Where(s => s.Index >= currentSongIndex)
        .OrderBy(s => s.Index)
        .ToList();

      foreach (var song in upcomingSongs)
      {
        queue.Enqueue(song);
      }
    }

    public SongQueue GetQueue(int teamId)
    {
      if (!_queues.ContainsKey(teamId))
        _queues[teamId] = new SongQueue();

      return _queues[teamId];
    }

    public void ClearQueue(int teamId)
    {
      if (_queues.ContainsKey(teamId))
        _queues[teamId].Clear();
    }

    public List<Song> GetQueueAsList(int teamId)
    {
      var queue = GetQueue(teamId);
      return queue.ToList();
    }

    public void AddToQueue(int teamId, Song song, bool insertAfterCurrent = false)
    {
      var queue = GetQueue(teamId);
      queue.Enqueue(song, insertAfterCurrent);
    }

    public bool RemoveFromQueue(int teamId, int songId)
    {
      var queue = GetQueue(teamId);
      var song = queue.FirstOrDefault(s => s.Id == songId);
      if (song != null)
      {
        return queue.Remove(song);
      }
      return false;
    }
  }
}