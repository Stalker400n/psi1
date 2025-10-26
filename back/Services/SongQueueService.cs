using System.Collections.Generic;
using System.Linq;
using back.Models;

namespace back.Services
{
  public class SongQueueService
  {
    private readonly Dictionary<int, SongQueue> _queues = new Dictionary<int, SongQueue>();

    // Initialize queue from database songs starting from currentSongIndex
    public void InitializeQueue(int teamId, IEnumerable<Song> allSongs, int currentSongIndex)
    {
      var queue = GetQueue(teamId);
      queue.Clear();

      // Get songs from currentSongIndex onwards, ordered by Index
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

    // Get all songs in the queue (from current to end)
    public List<Song> GetQueueAsList(int teamId)
    {
      var queue = GetQueue(teamId);
      return queue.ToList();
    }

    // Add song to queue at the appropriate position
    public void AddToQueue(int teamId, Song song, bool insertAfterCurrent = false)
    {
      var queue = GetQueue(teamId);
      queue.Enqueue(song, insertAfterCurrent);
    }

    // Remove song from queue
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