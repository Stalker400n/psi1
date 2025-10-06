using Microsoft.AspNetCore.Mvc;
using back.Models;
using back.DataSources;

namespace back.Controllers;

[ApiController]
[Route("api/teams/{teamId}/users")]
public class UsersController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<User>> GetUsers(int teamId)
    {
        var team = TeamStore.Teams.FirstOrDefault(t => t.Id == teamId);
        if (team == null) return NotFound(new { message = "Team not found" });
        return Ok(team.Users);
    }

    [HttpGet("{id}")]
    public ActionResult<User> GetUser(int teamId, int id)
    {
        var team = TeamStore.Teams.FirstOrDefault(t => t.Id == teamId);
        if (team == null) return NotFound(new { message = "Team not found" });
        
        var user = team.Users.FirstOrDefault(u => u.Id == id);
        if (user == null) return NotFound(new { message = "User not found" });
        
        return Ok(user);
    }

    [HttpPost]
    public ActionResult<User> AddUser(int teamId, [FromBody] User user)
    {
        var team = TeamStore.Teams.FirstOrDefault(t => t.Id == teamId);
        if (team == null) return NotFound(new { message = "Team not found" });

        user.Id = team.Users.Count > 0 ? team.Users.Max(u => u.Id) + 1 : 1;
        team.Users.Add(user);
        
        return CreatedAtAction(nameof(GetUser), new { teamId = teamId, id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public ActionResult<User> UpdateUser(int teamId, int id, [FromBody] User user)
    {
        var team = TeamStore.Teams.FirstOrDefault(t => t.Id == teamId);
        if (team == null) return NotFound(new { message = "Team not found" });
        
        var existingUser = team.Users.FirstOrDefault(u => u.Id == id);
        if (existingUser == null) return NotFound(new { message = "User not found" });
        
        // Update user properties
        existingUser.Name = user.Name;
        
        return Ok(existingUser);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteUser(int teamId, int id)
    {
        var team = TeamStore.Teams.FirstOrDefault(t => t.Id == teamId);
        if (team == null) return NotFound(new { message = "Team not found" });

        var user = team.Users.FirstOrDefault(u => u.Id == id);
        if (user == null) return NotFound(new { message = "User not found" });

        team.Users.Remove(user);
        return NoContent();
    }
}
