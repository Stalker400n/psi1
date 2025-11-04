using System.Collections;
using back.Models;

namespace back.Services
{
  public class SongQueue : IEnumerable<Song>
  {
    private readonly List<Song> _songs = new List<Song>();
    private readonly object _lock = new object();
    public void Enqueue(Song song, bool insertAtFront = false)
    {
      if (song == null) return;

      lock (_lock)
      {
        if (insertAtFront)
          _songs.Insert(0, song);
        else
          _songs.Add(song);
      }
    }

    public Song? Dequeue()
    {
      lock (_lock)
      {
        if (_songs.Count == 0) return null;
        var song = _songs[0];
        _songs.RemoveAt(0);
        return song;
      }
    }

    public Song? Peek()
    {
      lock (_lock)
      {
        return _songs.Count > 0 ? _songs[0] : null;
      }
    }
    public bool Remove(Song song)
    {
      lock (_lock)
      {
        return _songs.Remove(song);
      }
    }

    public void Clear()
    {
      lock (_lock)
      {
        _songs.Clear();
      }
    }

    public int Count
    {
      get
      {
        lock (_lock)
        {
          return _songs.Count;
        }
      }
    }

    public void UpdateSong(int songId, Song updatedSong)
    {
      lock (_lock)
      {
        var index = _songs.FindIndex(s => s.Id == songId);
        if (index != -1)
        {
          _songs[index] = updatedSong;
        }
      }
    }
    public List<Song> ToList()
    {
      lock (_lock)
      {
        return new List<Song>(_songs);
      }
    }

    public IEnumerator<Song> GetEnumerator() => ToList().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }
}
