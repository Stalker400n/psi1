using Microsoft.AspNetCore.Mvc;
using back.Models;
using System.Security.Cryptography;
using back.Data;
using Microsoft.EntityFrameworkCore;

namespace back.Controllers;

[ApiController]
[Route("teams")]
public class TeamsController : ControllerBase
{
  private readonly ApplicationDbContext _context;

  public TeamsController(ApplicationDbContext context)
  {
    _context = context;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<Team>>> GetTeams()
  {
    return Ok(await _context.Teams
        .Include(t => t.Users)
        .Include(t => t.Songs)
        .Include(t => t.Messages)
        .ToListAsync());
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<Team>> GetTeam(int id)
  {
    var team = await _context.Teams
        .Include(t => t.Users)
        .Include(t => t.Songs)
        .Include(t => t.Messages)
        .FirstOrDefaultAsync(t => t.Id == id);

    if (team == null)
    {
      return NotFound(new { message = "Team not found" });
    }
    return Ok(team);
  }

  [HttpPost]
  public async Task<ActionResult<Team>> CreateTeam([FromBody] Team team)
  {
    int newId;
    do
    {
      newId = RandomNumberGenerator.GetInt32(100000, 1000000);
    }
    while (await _context.Teams.AnyAsync(t => t.Id == newId));

    team.Id = newId;
    _context.Teams.Add(team);
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, team);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<Team>> UpdateTeam(int id, [FromBody] Team team)
  {
    var currentTeam = await _context.Teams.FindAsync(id);
    if (currentTeam == null)
    {
      return NotFound(new { message = "Team not found" });
    }

    currentTeam.Name = team.Name;
    currentTeam.IsPrivate = team.IsPrivate;

    _context.Entry(currentTeam).State = EntityState.Modified;
    await _context.SaveChangesAsync();

    return Ok(currentTeam);
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteTeam(int id)
  {
    var team = await _context.Teams.FindAsync(id);
    if (team == null)
    {
      return NotFound(new { message = "Team not found" });
    }

    _context.Teams.Remove(team);
    await _context.SaveChangesAsync();
    return NoContent();
  }
}
