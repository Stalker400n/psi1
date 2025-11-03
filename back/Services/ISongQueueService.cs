using back.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace back.Services
{
  public interface ISongQueueService
  {
    Task InitializeQueueAsync(int teamId);
    Task<List<Song>> GetQueueAsync(int teamId);
    Task<Song?> GetCurrentSongAsync(int teamId);
    Task<Song?> AdvanceToNextSongAsync(int teamId);
    Task<Song?> GoToPreviousSongAsync(int teamId);
    Task<Song?> JumpToSongAsync(int teamId, int index);
    Task RefreshQueueAsync(int teamId);
  }
}
