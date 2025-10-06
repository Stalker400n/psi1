using Microsoft.AspNetCore.Mvc;
using back.Models;
using back.Data;
using Microsoft.EntityFrameworkCore;

namespace back.Controllers;

[ApiController]
[Route("teams/{teamId}/users")]
public class UsersController : ControllerBase
{
  private readonly ApplicationDbContext _context;

  public UsersController(ApplicationDbContext context)
  {
    _context = context;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<User>>> GetUsers(int teamId)
  {
    var team = await _context.Teams
        .Include(t => t.Users)
        .FirstOrDefaultAsync(t => t.Id == teamId);

    if (team == null) return NotFound(new { message = "Team not found" });
    return Ok(team.Users);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<User>> GetUser(int teamId, int id)
  {
    var team = await _context.Teams
        .Include(t => t.Users)
        .FirstOrDefaultAsync(t => t.Id == teamId);

    if (team == null) return NotFound(new { message = "Team not found" });

    var user = team.Users.FirstOrDefault(u => u.Id == id);
    if (user == null) return NotFound(new { message = "User not found" });

    return Ok(user);
  }

  [HttpPost]
  public async Task<ActionResult<User>> AddUser(int teamId, [FromBody] User user)
  {
    var team = await _context.Teams
        .Include(t => t.Users)
        .FirstOrDefaultAsync(t => t.Id == teamId);

    if (team == null) return NotFound(new { message = "Team not found" });

    _context.Users.Add(user);
    await _context.SaveChangesAsync();

    team.Users.Add(user);
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(GetUser), new { teamId = teamId, id = user.Id }, user);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<User>> UpdateUser(int teamId, int id, [FromBody] User user)
  {
    var team = await _context.Teams
        .Include(t => t.Users)
        .FirstOrDefaultAsync(t => t.Id == teamId);

    if (team == null) return NotFound(new { message = "Team not found" });

    var existingUser = team.Users.FirstOrDefault(u => u.Id == id);
    if (existingUser == null) return NotFound(new { message = "User not found" });

    existingUser.Name = user.Name;
    existingUser.Score = user.Score;
    existingUser.IsActive = user.IsActive;

    _context.Entry(existingUser).State = EntityState.Modified;
    await _context.SaveChangesAsync();

    return Ok(existingUser);
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteUser(int teamId, int id)
  {
    var team = await _context.Teams
        .Include(t => t.Users)
        .FirstOrDefaultAsync(t => t.Id == teamId);

    if (team == null) return NotFound(new { message = "Team not found" });

    var user = team.Users.FirstOrDefault(u => u.Id == id);
    if (user == null) return NotFound(new { message = "User not found" });

    team.Users.Remove(user);
    await _context.SaveChangesAsync();

    return NoContent();
  }
}
