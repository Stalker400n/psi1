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
    private readonly ITeamsRepository _teamsRepository;
    private readonly SongQueueService _queueService;

    public SongsController(
      ISongsRepository songsRepository,
      ITeamsRepository teamsRepository,
      SongQueueService queueService)
    {
      _songsRepository = songsRepository;
      _teamsRepository = teamsRepository;
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
    public async Task<ActionResult<Song>> AddSong(
      int teamId,
      [FromBody] Song song,
      [FromQuery] bool insertAfterCurrent = false)
    {
      var team = await _teamsRepository.GetByIdAsync(teamId);
      if (team == null) return NotFound(new { message = "Team not found" });

      // Determine the index for the new song
      if (insertAfterCurrent)
      {
        // Insert right after current song
        song.Index = team.CurrentSongIndex + 1;

        // Shift all subsequent songs by 1
        var songsToShift = team.Songs
          .Where(s => s.Index > team.CurrentSongIndex)
          .ToList();

        foreach (var s in songsToShift)
        {
          s.Index++;
          await _songsRepository.UpdateSongAsync(teamId, s.Id, s);
        }
      }
      else
      {
        // Add to the end
        var maxIndex = team.Songs.Any() ? team.Songs.Max(s => s.Index) : -1;
        song.Index = maxIndex + 1;
      }

      var created = await _songsRepository.AddSongAsync(teamId, song);
      if (created == null) return NotFound(new { message = "Failed to add song" });

      // Reinitialize the queue with updated songs
      var allSongs = await _songsRepository.GetSongsAsync(teamId);
      if (allSongs != null)
      {
        _queueService.InitializeQueue(teamId, allSongs, team.CurrentSongIndex);
      }

      return CreatedAtAction(nameof(GetSong), new { teamId = teamId, id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Song>> UpdateSong(int teamId, int id, [FromBody] Song song)
    {
      var updated = await _songsRepository.UpdateSongAsync(teamId, id, song);
      if (updated == null) return NotFound(new { message = "Team or song not found" });

      // Update the song in the queue if it exists
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
      var team = await _teamsRepository.GetByIdAsync(teamId);
      if (team == null) return NotFound(new { message = "Team not found" });

      var songToDelete = team.Songs.FirstOrDefault(s => s.Id == id);
      if (songToDelete == null) return NotFound(new { message = "Song not found" });

      var deleted = await _songsRepository.DeleteSongAsync(teamId, id);
      if (!deleted) return NotFound(new { message = "Failed to delete song" });

      // Shift down all songs after the deleted one
      var songsToShift = team.Songs
        .Where(s => s.Index > songToDelete.Index)
        .ToList();

      foreach (var s in songsToShift)
      {
        s.Index--;
        await _songsRepository.UpdateSongAsync(teamId, s.Id, s);
      }

      // Reinitialize the queue
      var allSongs = await _songsRepository.GetSongsAsync(teamId);
      if (allSongs != null)
      {
        _queueService.InitializeQueue(teamId, allSongs, team.CurrentSongIndex);
      }

      return NoContent();
    }

    // Get the queue (songs from current to end)
    [HttpGet("queue")]
    public async Task<ActionResult<IEnumerable<Song>>> GetQueue(int teamId)
    {
      var team = await _teamsRepository.GetByIdAsync(teamId);
      if (team == null) return NotFound(new { message = "Team not found" });

      // Initialize queue if empty
      var queue = _queueService.GetQueue(teamId);
      if (queue.Count == 0)
      {
        var allSongs = await _songsRepository.GetSongsAsync(teamId);
        if (allSongs != null)
        {
          _queueService.InitializeQueue(teamId, allSongs, team.CurrentSongIndex);
        }
      }

      return Ok(_queueService.GetQueueAsList(teamId));
    }

    // Get the current song
    [HttpGet("current")]
    public async Task<ActionResult<Song>> GetCurrentSong(int teamId)
    {
      var team = await _teamsRepository.GetByIdAsync(teamId);
      if (team == null) return NotFound(new { message = "Team not found" });

      var allSongs = await _songsRepository.GetSongsAsync(teamId);
      if (allSongs == null) return NotFound(new { message = "No songs found" });

      var currentSong = allSongs.FirstOrDefault(s => s.Index == team.CurrentSongIndex);

      if (currentSong == null)
        return NotFound(new { message = "No current song" });

      return Ok(currentSong);
    }

    // Move to the next song
    [HttpPost("next")]
    public async Task<ActionResult<Song>> NextSong(int teamId)
    {
      var team = await _teamsRepository.GetByIdAsync(teamId);
      if (team == null) return NotFound(new { message = "Team not found" });

      var allSongs = await _songsRepository.GetSongsAsync(teamId);
      if (allSongs == null) return NotFound(new { message = "No songs found" });

      var songsList = allSongs.ToList();
      var maxIndex = songsList.Any() ? songsList.Max(s => s.Index) : -1;

      if (team.CurrentSongIndex >= maxIndex)
        return NotFound(new { message = "No more songs in queue" });

      // Move to next song
      team.CurrentSongIndex++;
      await _teamsRepository.UpdateAsync(teamId, team);

      // Reinitialize queue from new current index
      _queueService.InitializeQueue(teamId, allSongs, team.CurrentSongIndex);

      var currentSong = songsList.FirstOrDefault(s => s.Index == team.CurrentSongIndex);
      return Ok(currentSong);
    }

    // Move to the previous song
    [HttpPost("previous")]
    public async Task<ActionResult<Song>> PreviousSong(int teamId)
    {
      var team = await _teamsRepository.GetByIdAsync(teamId);
      if (team == null) return NotFound(new { message = "Team not found" });

      if (team.CurrentSongIndex <= 0)
        return NotFound(new { message = "Already at first song" });

      // Move to previous song
      team.CurrentSongIndex--;
      await _teamsRepository.UpdateAsync(teamId, team);

      // Reinitialize queue from new current index
      var allSongs = await _songsRepository.GetSongsAsync(teamId);
      if (allSongs != null)
      {
        _queueService.InitializeQueue(teamId, allSongs, team.CurrentSongIndex);
      }

      var currentSong = allSongs?.FirstOrDefault(s => s.Index == team.CurrentSongIndex);
      return Ok(currentSong);
    }

    // Jump to a specific song by index
    [HttpPost("jump/{index}")]
    public async Task<ActionResult<Song>> JumpToSong(int teamId, int index)
    {
      var team = await _teamsRepository.GetByIdAsync(teamId);
      if (team == null) return NotFound(new { message = "Team not found" });

      var allSongs = await _songsRepository.GetSongsAsync(teamId);
      if (allSongs == null) return NotFound(new { message = "No songs found" });

      var targetSong = allSongs.FirstOrDefault(s => s.Index == index);

      if (targetSong == null)
        return NotFound(new { message = "Song at specified index not found" });

      // Update current index
      team.CurrentSongIndex = index;
      await _teamsRepository.UpdateAsync(teamId, team);

      // Reinitialize queue from new current index
      _queueService.InitializeQueue(teamId, allSongs, team.CurrentSongIndex);

      return Ok(targetSong);
    }
  }
}