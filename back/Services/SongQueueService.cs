using System.Collections;
using System.Collections.Generic;
using System.Linq;
using back.Models;

namespace back.Services
{
  public class SongQueue : IEnumerable<Song>
  {
    private readonly List<Song> _songs = new List<Song>();

    public void Enqueue(Song song)
    {
      if (song != null)
        _songs.Add(song);
    }

    public Song? Dequeue()
    {
      if (_songs.Count == 0) return null;

      var song = _songs[0];
      _songs.RemoveAt(0);
      return song;
    }

    public Song? Peek()
    {
      return _songs.Count > 0 ? _songs[0] : null;
    }

    public bool Remove(Song song)
    {
      return _songs.Remove(song);
    }

    public void Clear()
    {
      _songs.Clear();
    }

    public int Count => _songs.Count;

    public IEnumerator<Song> GetEnumerator() => _songs.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerable<Song> TopRated(int minRating)
    {
      return _songs.Where(s => s.Rating >= minRating);
    }
  }
}
