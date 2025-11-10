using back.Models;

namespace back.Data.Repositories;

public interface IRatingsRepository
{
  Task<IEnumerable<SongRating>> GetSongRatingsAsync(int teamId, int songId);
  
  Task<SongRating?> GetUserRatingAsync(int teamId, int songId, int userId);
  
  Task<SongRating?> AddRatingAsync(int teamId, int songId, SongRating rating);
  
  Task<SongRating?> UpdateRatingAsync(int teamId, int songId, int ratingId, SongRating rating);
  
  Task<bool> DeleteRatingAsync(int teamId, int songId, int ratingId);
}