using Microsoft.AspNetCore.Mvc;
using back.Models;
using back.Data.Repositories;

namespace back.Controllers;

[ApiController]
[Route("teams/{teamId}/songs")]
public class SongsController : ControllerBase
{
  private readonly ISongsRepository _songsRepository;

  public SongsController(ISongsRepository songsRepository)
  {
    _songsRepository = songsRepository;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<Song>>> GetSongs(int teamId)
  {
    var songs = await _songsRepository.GetSongsAsync(teamId);
    if (songs == null) return NotFound(new { message = "Team not found" });
    return Ok(songs);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<Song>> GetSong(int teamId, int id)
  {
    var song = await _songsRepository.GetSongAsync(teamId, id);
    if (song == null) return NotFound(new { message = "Team or song not found" });
    return Ok(song);
  }

  [HttpPost]
  public async Task<ActionResult<Song>> AddSong(int teamId, [FromBody] Song song)
  {
    var created = await _songsRepository.AddSongAsync(teamId, song);
    if (created == null) return NotFound(new { message = "Team not found" });
    return CreatedAtAction(nameof(GetSong), new { teamId = teamId, id = created.Id }, created);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<Song>> UpdateSong(int teamId, int id, [FromBody] Song song)
  {
    var updated = await _songsRepository.UpdateSongAsync(teamId, id, song);
    if (updated == null) return NotFound(new { message = "Team or song not found" });
    return Ok(updated);
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteSong(int teamId, int id)
  {
    var deleted = await _songsRepository.DeleteSongAsync(teamId, id);
    if (!deleted) return NotFound(new { message = "Team or song not found" });
    return NoContent();
  }
}
