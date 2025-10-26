using Microsoft.AspNetCore.Mvc;
using back.Models;
using back.Data.Repositories;
using back.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back.Controllers
{
  [ApiController]
  [Route("teams/{teamId}/songs")]
  public class SongsController : ControllerBase
  {
    private readonly ISongsRepository _songsRepository;
    private readonly SongQueueService _queueService;

    public SongsController(ISongsRepository songsRepository, SongQueueService queueService)
    {
      _songsRepository = songsRepository;
      _queueService = queueService;
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

      _queueService.GetQueue(teamId).Enqueue(created);

      return CreatedAtAction(nameof(GetSong), new { teamId = teamId, id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Song>> UpdateSong(int teamId, int id, [FromBody] Song song)
    {
      var updated = await _songsRepository.UpdateSongAsync(teamId, id, song);
      if (updated == null) return NotFound(new { message = "Team or song not found" });

      var queue = _queueService.GetQueue(teamId);
      var queuedSong = queue.FirstOrDefault(s => s.Id == id);
      if (queuedSong != null)
      {
        queuedSong.Title = updated.Title;
        queuedSong.Artist = updated.Artist;
        queuedSong.Link = updated.Link;
        queuedSong.Rating = updated.Rating;
      }

      return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSong(int teamId, int id)
    {
      var deleted = await _songsRepository.DeleteSongAsync(teamId, id);
      if (!deleted) return NotFound(new { message = "Team or song not found" });

      var queue = _queueService.GetQueue(teamId);
      var queuedSong = queue.FirstOrDefault(s => s.Id == id);
      if (queuedSong != null)
        queue.Remove(queuedSong);

      return NoContent();
    }

    [HttpGet("queue")]
    public ActionResult<IEnumerable<Song>> GetQueue(int teamId)
    {
      var queue = _queueService.GetQueue(teamId);
      return Ok(queue.ToList());
    }

    [HttpPost("queue/dequeue")]
    public ActionResult<Song?> DequeueSong(int teamId)
    {
      var queue = _queueService.GetQueue(teamId);
      var song = queue.Dequeue();
      if (song == null) return NotFound(new { message = "Queue is empty" });
      return Ok(song);
    }
  }
}
