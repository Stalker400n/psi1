using back.Services;
using System.Collections.Concurrent;

namespace back.Cache
{
  public interface ISongQueuesCache
  {
      ConcurrentDictionary<int, SongQueue> Queues { get;  }
  }
}