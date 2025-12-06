using Microsoft.AspNetCore.Mvc;
using back.Models;
using back.Data.Repositories;

namespace back.Controllers;

  [ApiController]
  [Route("teams")]
  public class TeamsController : ControllerBase
  {
    private readonly ITeamsRepository _teamsRepository;
    private readonly ILogger<TeamsController> _logger;

    public TeamsController(
      ITeamsRepository teamsRepository,
      ILogger<TeamsController> logger)
    {
      if (teamsRepository == null) throw new ArgumentNullException(nameof(teamsRepository));
      if (logger == null) throw new ArgumentNullException(nameof(logger));
      
      _teamsRepository = teamsRepository;
      _logger = logger;
    }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<Team>>> GetTeams()
  {
    var teams = await _teamsRepository.GetAllAsync();
    return Ok(teams);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<Team>> GetTeam(int id)
  {
    var team = await _teamsRepository.GetByIdAsync(id);
    if (team == null) return NotFound(new { message = "Team not found" });
    return Ok(team);
  }

  [HttpPost]
  public async Task<ActionResult<Team>> CreateTeam([FromBody] Team team)
  {
    var created = await _teamsRepository.CreateAsync(team);
    return CreatedAtAction(nameof(GetTeam), new { id = created.Id }, created);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<Team>> UpdateTeam(int id, [FromBody] Team team)
  {
    var updated = await _teamsRepository.UpdateAsync(id, team);
    if (updated == null) return NotFound(new { message = "Team not found" });
    return Ok(updated);
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteTeam(int id)
  {
    var deleted = await _teamsRepository.DeleteAsync(id);
    if (!deleted) return NotFound(new { message = "Team not found" });
    return NoContent();
  }

  [HttpGet("{id}/queue-history")]
  public async Task<ActionResult<object>> GetQueueHistory(int id)
  {
    try
    {
      var team = await _teamsRepository.GetByIdAsync(id);
      if (team == null) return NotFound(new { message = "Team not found" });
      
      var allSongs = team.Songs.OrderBy(s => s.Index).ToList();
      
      return Ok(new
      {
        currentIndex = team.CurrentSongIndex,
        songs = allSongs
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error getting queue history for team {Id}", id);
      return StatusCode(500, new { message = "An error occurred while fetching queue history" });
    }
  }
}
