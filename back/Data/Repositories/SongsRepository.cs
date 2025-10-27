using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back.Models;
using Microsoft.EntityFrameworkCore;

namespace back.Data.Repositories;

public class SongsRepository : ISongsRepository
{
  private readonly ApplicationDbContext _context;

  public SongsRepository(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task<IEnumerable<Song>?> GetSongsAsync(int teamId)
  {
    var team = await _context.Teams
        .Include(t => t.Songs)
        .FirstOrDefaultAsync(t => t.Id == teamId);

    if (team == null) return null;

    return team.Songs.OrderBy(s => s.Index).ToList();
  }

  public async Task<Song?> GetSongAsync(int teamId, int songId)
  {
    var team = await _context.Teams
        .Include(t => t.Songs)
        .FirstOrDefaultAsync(t => t.Id == teamId);

    if (team == null) return null;

    return team.Songs.FirstOrDefault(s => s.Id == songId);
  }

  public async Task<Song?> AddSongAsync(int teamId, Song song)
  {
    var team = await _context.Teams
        .Include(t => t.Songs)
        .FirstOrDefaultAsync(t => t.Id == teamId);

    if (team == null) return null;

    _context.Songs.Add(song);
    await _context.SaveChangesAsync();

    team.Songs.Add(song);
    await _context.SaveChangesAsync();

    return song;
  }

  public async Task<Song?> UpdateSongAsync(int teamId, int songId, Song song)
  {
    var team = await _context.Teams
        .Include(t => t.Songs)
        .FirstOrDefaultAsync(t => t.Id == teamId);

    if (team == null) return null;

    var existingSong = team.Songs.FirstOrDefault(s => s.Id == songId);
    if (existingSong == null) return null;

    var updatedSong = existingSong with
    {
      Rating = song.Rating,
      Index = song.Index
    };

    team.Songs.Remove(existingSong);
    team.Songs.Add(updatedSong);

    _context.Entry(existingSong).State = EntityState.Detached;
    _context.Songs.Update(updatedSong);
    await _context.SaveChangesAsync();

    return updatedSong;
  }

  public async Task<bool> DeleteSongAsync(int teamId, int songId)
  {
    var team = await _context.Teams
        .Include(t => t.Songs)
        .FirstOrDefaultAsync(t => t.Id == teamId);

    if (team == null) return false;

    var song = team.Songs.FirstOrDefault(s => s.Id == songId);
    if (song == null) return false;

    team.Songs.Remove(song);
    _context.Songs.Remove(song);
    await _context.SaveChangesAsync();

    return true;
  }
}