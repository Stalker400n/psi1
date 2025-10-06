using Microsoft.AspNetCore.Mvc;
using back.Models;
using back.Data;
using Microsoft.EntityFrameworkCore;

namespace back.Controllers;

[ApiController]
[Route("teams/{teamId}/songs")]
public class SongsController : ControllerBase
{
  private readonly ApplicationDbContext _context;

  public SongsController(ApplicationDbContext context)
  {
    _context = context;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<Song>>> GetSongs(int teamId)
  {
    var team = await _context.Teams
        .Include(t => t.Songs)
        .FirstOrDefaultAsync(t => t.Id == teamId);

    if (team == null) return NotFound(new { message = "Team not found" });
    return Ok(team.Songs);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<Song>> GetSong(int teamId, int id)
  {
    var team = await _context.Teams
        .Include(t => t.Songs)
        .FirstOrDefaultAsync(t => t.Id == teamId);

    if (team == null) return NotFound(new { message = "Team not found" });

    var song = team.Songs.FirstOrDefault(s => s.Id == id);
    if (song == null) return NotFound(new { message = "Song not found" });

    return Ok(song);
  }

  [HttpPost]
  public async Task<ActionResult<Song>> AddSong(int teamId, [FromBody] Song song)
  {
    var team = await _context.Teams
        .Include(t => t.Songs)
        .FirstOrDefaultAsync(t => t.Id == teamId);

    if (team == null) return NotFound(new { message = "Team not found" });

    if (song.Id == 0)
    {
      song.Id = team.Songs.Count > 0 ? team.Songs.Max(s => s.Id) + 1 : 1;
    }

    team.Songs.Add(song);
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(GetSong), new { teamId = teamId, id = song.Id }, song);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<Song>> UpdateSong(int teamId, int id, [FromBody] Song song)
  {
    var team = await _context.Teams
        .Include(t => t.Songs)
        .FirstOrDefaultAsync(t => t.Id == teamId);

    if (team == null) return NotFound(new { message = "Team not found" });

    var existingSong = team.Songs.FirstOrDefault(s => s.Id == id);
    if (existingSong == null) return NotFound(new { message = "Song not found" });

    existingSong.Link = song.Link;
    existingSong.Title = song.Title;
    existingSong.Artist = song.Artist;
    existingSong.Rating = song.Rating;

    _context.Entry(existingSong).State = EntityState.Modified;
    await _context.SaveChangesAsync();

    return Ok(existingSong);
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteSong(int teamId, int id)
  {
    var team = await _context.Teams
        .Include(t => t.Songs)
        .FirstOrDefaultAsync(t => t.Id == teamId);

    if (team == null) return NotFound(new { message = "Team not found" });

    var song = team.Songs.FirstOrDefault(s => s.Id == id);
    if (song == null) return NotFound(new { message = "Song not found" });

    team.Songs.Remove(song);
    await _context.SaveChangesAsync();

    return NoContent();
  }
}
