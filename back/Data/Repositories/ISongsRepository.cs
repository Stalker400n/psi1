using back.Models;

namespace back.Data.Repositories;

public interface ISongsRepository
{
	Task<IEnumerable<Song>> GetSongsAsync(int teamId);

	Task<Song?> GetSongAsync(int teamId, int songId);

	Task<Song> AddSongAsync(int teamId, Song song);

	Task<Song?> UpdateSongAsync(int teamId, int songId, Song song);

	Task<bool> DeleteSongAsync(int teamId, int songId);
}

