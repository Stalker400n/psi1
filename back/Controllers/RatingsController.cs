using Microsoft.AspNetCore.Mvc;
using back.Models;
using back.Data.Repositories;

namespace back.Controllers
{
  [ApiController]
  [Route("teams/{teamId}/songs/{songId}/ratings")]
  public class RatingsController : ControllerBase
  {
    private readonly IRatingsRepository _ratingsRepository;
    private readonly ISongsRepository _songsRepository;
    private readonly ILogger<RatingsController> _logger;

    public RatingsController(
      IRatingsRepository ratingsRepository,
      ISongsRepository songsRepository,
      ILogger<RatingsController> logger)
    {
      if (ratingsRepository == null) throw new ArgumentNullException(nameof(ratingsRepository));
      if (songsRepository == null) throw new ArgumentNullException(nameof(songsRepository));
      if (logger == null) throw new ArgumentNullException(nameof(logger));
      
      _ratingsRepository = ratingsRepository;
      _songsRepository = songsRepository;
      _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SongRating>>> GetRatings(int teamId, int songId)
    {
      var song = await _songsRepository.GetSongAsync(teamId, songId);
      if (song == null) return NotFound(new { message = "Song not found" });

      var ratings = await _ratingsRepository.GetSongRatingsAsync(teamId, songId);
      return Ok(ratings);
    }

    [HttpPost]
    public async Task<ActionResult<SongRating>> SubmitRating(
      int teamId,
      int songId,
      [FromBody] SongRating rating)
    {
      var song = await _songsRepository.GetSongAsync(teamId, songId);
      if (song == null) return NotFound(new { message = "Song not found" });

      rating.SongId = songId;
      rating.UpdatedAt = DateTime.UtcNow;

      var existingRating = await _ratingsRepository.GetUserRatingAsync(teamId, songId, rating.UserId);

      SongRating result;
      if (existingRating != null)
      {
        result = await _ratingsRepository.UpdateRatingAsync(teamId, songId, existingRating.Id, rating);
      }
      else
      {
        rating.CreatedAt = DateTime.UtcNow;
        result = await _ratingsRepository.AddRatingAsync(teamId, songId, rating);
      }

      if (result == null)
        return BadRequest(new { message = "Failed to submit rating" });

      return Ok(result);
    }

    [HttpDelete("{ratingId}")]
    public async Task<IActionResult> DeleteRating(int teamId, int songId, int ratingId)
    {
      var deleted = await _ratingsRepository.DeleteRatingAsync(teamId, songId, ratingId);
      if (!deleted) return NotFound(new { message = "Rating not found" });

      return NoContent();
    }
  }
}