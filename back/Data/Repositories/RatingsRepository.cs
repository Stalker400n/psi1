using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back.Models;
using Microsoft.EntityFrameworkCore;

namespace back.Data.Repositories;

public class RatingsRepository : IRatingsRepository
{
  private readonly ApplicationDbContext _context;

  public RatingsRepository(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task<IEnumerable<SongRating>> GetSongRatingsAsync(int teamId, int songId)
  {
    var team = await _context.Teams
      .Include(t => t.Songs)
      .FirstOrDefaultAsync(t => t.Id == teamId);

    if (team == null) return new List<SongRating>();

    var song = team.Songs.FirstOrDefault(s => s.Id == songId);
    if (song == null) return new List<SongRating>();

    return await _context.SongRatings
      .Where(r => r.SongId == songId)
      .OrderByDescending(r => r.UpdatedAt)
      .ToListAsync();
  }

  public async Task<SongRating?> GetUserRatingAsync(int teamId, int songId, int userId)
  {
    var team = await _context.Teams
      .Include(t => t.Songs)
      .FirstOrDefaultAsync(t => t.Id == teamId);

    if (team == null) return null;

    var song = team.Songs.FirstOrDefault(s => s.Id == songId);
    if (song == null) return null;

    return await _context.SongRatings
      .FirstOrDefaultAsync(r => r.SongId == songId && r.UserId == userId);
  }

  public async Task<SongRating?> AddRatingAsync(int teamId, int songId, SongRating rating)
  {
    var team = await _context.Teams
      .Include(t => t.Songs)
      .FirstOrDefaultAsync(t => t.Id == teamId);

    if (team == null) return null;

    var song = team.Songs.FirstOrDefault(s => s.Id == songId);
    if (song == null) return null;

    rating.SongId = songId;
    rating.CreatedAt = DateTime.UtcNow;
    rating.UpdatedAt = DateTime.UtcNow;

    _context.SongRatings.Add(rating);
    await _context.SaveChangesAsync();

    return rating;
  }

  public async Task<SongRating?> UpdateRatingAsync(int teamId, int songId, int ratingId, SongRating rating)
  {
    var team = await _context.Teams
      .Include(t => t.Songs)
      .FirstOrDefaultAsync(t => t.Id == teamId);

    if (team == null) return null;

    var song = team.Songs.FirstOrDefault(s => s.Id == songId);
    if (song == null) return null;

    var existingRating = await _context.SongRatings
      .FirstOrDefaultAsync(r => r.Id == ratingId && r.SongId == songId);

    if (existingRating == null) return null;

    existingRating.Rating = rating.Rating;
    existingRating.UpdatedAt = DateTime.UtcNow;

    _context.Entry(existingRating).State = EntityState.Modified;
    await _context.SaveChangesAsync();

    return existingRating;
  }

  public async Task<bool> DeleteRatingAsync(int teamId, int songId, int ratingId)
  {
    var team = await _context.Teams
      .Include(t => t.Songs)
      .FirstOrDefaultAsync(t => t.Id == teamId);

    if (team == null) return false;

    var song = team.Songs.FirstOrDefault(s => s.Id == songId);
    if (song == null) return false;

    var rating = await _context.SongRatings
      .FirstOrDefaultAsync(r => r.Id == ratingId && r.SongId == songId);

    if (rating == null) return false;

    _context.SongRatings.Remove(rating);
    await _context.SaveChangesAsync();

    return true;
  }
}