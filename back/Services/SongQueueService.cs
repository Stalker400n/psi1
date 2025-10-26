using System.Collections.Generic;

namespace back.Services
{
  public class SongQueueService
  {
    private readonly Dictionary<int, SongQueue> _queues = new Dictionary<int, SongQueue>();

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
  }
}
