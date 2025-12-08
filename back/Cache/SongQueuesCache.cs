using back.Services;
using System.Collections.Concurrent;

namespace back.Cache
{
  public class SongQueuesCache : ISongQueuesCache
  {
    public ConcurrentDictionary<int, SongQueue> Queues { get; } = new ConcurrentDictionary<int, SongQueue>();

    public SongQueuesCache()
    {
      Queues = new ConcurrentDictionary<int, SongQueue>();  
    }
  }
}