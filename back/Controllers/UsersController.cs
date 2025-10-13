using Microsoft.AspNetCore.Mvc;
using back.Models;
using back.Data.Repositories;

namespace back.Controllers;

[ApiController]
[Route("teams/{teamId}/users")]
public class UsersController : ControllerBase
{
  private readonly IUsersRepository _usersRepository;

  public UsersController(IUsersRepository usersRepository)
  {
    _usersRepository = usersRepository;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<User>>> GetUsers(int teamId)
  {
    var users = await _usersRepository.GetUsersAsync(teamId);
    if (users == null) return NotFound(new { message = "Team not found" });
    return Ok(users);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<User>> GetUser(int teamId, int id)
  {
    var user = await _usersRepository.GetUserAsync(teamId, id);
    if (user == null) return NotFound(new { message = "Team or user not found" });
    return Ok(user);
  }

  [HttpPost]
  public async Task<ActionResult<User>> AddUser(int teamId, [FromBody] User user)
  {
    var created = await _usersRepository.CreateUserAsync(teamId, user);
    if (created == null) return NotFound(new { message = "Team not found" });
    return CreatedAtAction(nameof(GetUser), new { teamId = teamId, id = created.Id }, created);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<User>> UpdateUser(int teamId, int id, [FromBody] User user)
  {
    var updated = await _usersRepository.UpdateUserAsync(teamId, id, user);
    if (updated == null) return NotFound(new { message = "Team or user not found" });
    return Ok(updated);
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteUser(int teamId, int id)
  {
    var deleted = await _usersRepository.DeleteUserAsync(teamId, id);
    if (!deleted) return NotFound(new { message = "Team or user not found" });
    return NoContent();
  }
}
