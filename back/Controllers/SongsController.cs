using Microsoft.AspNetCore.Mvc;
using back.Models;
using back.Data.Repositories;
using back.Services;
using back.Exceptions;
using back.Validators;

namespace back.Controllers
{
  [ApiController]
  [Route("teams/{teamId}/songs")]
  public class SongsController : ControllerBase
  {
    private readonly ISongsRepository _songsRepository;
    private readonly ITeamsRepository _teamsRepository;
    private readonly ISongQueueService _queueService;
    private readonly IYoutubeValidator _youtubeValidator;
    private readonly ILogger<SongsController> _logger;

    public SongsController(
      ISongsRepository songsRepository,
      ITeamsRepository teamsRepository,
      ISongQueueService queueService,
      IYoutubeValidator youtubeValidator,
      ILogger<SongsController> logger)
    {
      if (songsRepository == null) throw new ArgumentNullException(nameof(songsRepository));
      if (teamsRepository == null) throw new ArgumentNullException(nameof(teamsRepository));
      if (queueService == null) throw new ArgumentNullException(nameof(queueService));
      if (youtubeValidator == null) throw new ArgumentNullException(nameof(youtubeValidator));
      if (logger == null) throw new ArgumentNullException(nameof(logger));
      
      _songsRepository = songsRepository;
      _teamsRepository = teamsRepository;
      _queueService = queueService;
      _youtubeValidator = youtubeValidator;
      _logger = logger;
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
      try
      {
        await _youtubeValidator.ValidateLink(song.Link);
      }
      catch (YoutubeValidationException ex)
      {
        _logger.LogWarning(ex, "YouTube validation failed for link: {Link}", song.Link);
        return BadRequest(new { message = ex.Message });
      }

      var team = await _teamsRepository.GetByIdAsync(teamId);
      if (team == null) return NotFound(new { message = "Team not found" });

      if (insertAfterCurrent)
      {
        song.Index = team.CurrentSongIndex + 1;

        var songsToShift = team.Songs
          .Where(s => s.Index > team.CurrentSongIndex)
          .ToList();

        foreach (var s in songsToShift)
        {
          var shiftedSong = s with { Index = s.Index + 1 };
          await _songsRepository.UpdateSongAsync(teamId, s.Id, shiftedSong);
        }
      }
      else
      {
        var maxIndex = team.Songs.Any() ? team.Songs.Max(s => s.Index) : -1;
        song.Index = maxIndex + 1;
      }

      var created = await _songsRepository.AddSongAsync(teamId, song);
      if (created == null) return NotFound(new { message = "Failed to add song" });

      await _queueService.RefreshQueueAsync(teamId);

      return CreatedAtAction(nameof(GetSong), new { teamId = teamId, id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Song>> UpdateSong(int teamId, int id, [FromBody] Song song)
    {
      var updated = await _songsRepository.UpdateSongAsync(teamId, id, song);
      if (updated == null) return NotFound(new { message = "Team or song not found" });

      await _queueService.RefreshQueueAsync(teamId);

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

      var songsToShift = team.Songs
        .Where(s => s.Index > songToDelete.Index)
        .ToList();

      foreach (var s in songsToShift)
      {
        s.Index--;
        await _songsRepository.UpdateSongAsync(teamId, s.Id, s);
      }

      await _queueService.RefreshQueueAsync(teamId);

      return NoContent();
    }

    [HttpGet("queue")]
    public async Task<ActionResult<IEnumerable<Song>>> GetQueue(int teamId)
    {
      var team = await _teamsRepository.GetByIdAsync(teamId);
      if (team == null) return NotFound(new { message = "Team not found" });

      var queue = await _queueService.GetQueueAsync(teamId);
      return Ok(queue);
    }

    [HttpGet("current")]
    public async Task<ActionResult<Song>> GetCurrentSong(int teamId)
    {
      var team = await _teamsRepository.GetByIdAsync(teamId);
      if (team == null) return NotFound(new { message = "Team not found" });

      var currentSong = await _queueService.GetCurrentSongAsync(teamId);

      if (currentSong == null)
        return NotFound(new { message = "No current song" });

      return Ok(currentSong);
    }

    [HttpPost("next")]
    public async Task<ActionResult<Song>> NextSong(int teamId)
    {
      var nextSong = await _queueService.AdvanceToNextSongAsync(teamId);

      if (nextSong == null)
        return NotFound(new { message = "No more songs in queue" });

      return Ok(nextSong);
    }

    [HttpPost("previous")]
    public async Task<ActionResult<Song>> PreviousSong(int teamId)
    {
      var previousSong = await _queueService.GoToPreviousSongAsync(teamId);

      if (previousSong == null)
        return NotFound(new { message = "Already at first song" });

      return Ok(previousSong);
    }

    [HttpPost("jump/{index}")]
    public async Task<ActionResult<Song>> JumpToSong(int teamId, int index)
    {
      var targetSong = await _queueService.JumpToSongAsync(teamId, index);

      if (targetSong == null)
        return NotFound(new { message = "Song at specified index not found" });

      return Ok(targetSong);
    }
  }
}
