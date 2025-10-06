using Microsoft.AspNetCore.Mvc;
using back.Models;
using back.Data;

namespace back.Controllers;

[ApiController]
[Route("teams/{id}/users")]
public class TeamUsersController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<User>> GetUsers(int id)
    {
        var team = TeamStore.Teams.FirstOrDefault(t => t.Id == id);
        if (team == null) return NotFound();
        return Ok(team.Users);
    }

    [HttpPost]
    public ActionResult<User> AddUser(int id, [FromBody] User user)
    {
        var team = TeamStore.Teams.FirstOrDefault(t => t.Id == id);
        if (team == null) return NotFound();

        user.Id = team.Users.Count + 1;
        team.Users.Add(user);
        return Ok(user);
    }

    [HttpDelete("{userId}")]
    public IActionResult DeleteUser(int id, int userId)
    {
        var team = TeamStore.Teams.FirstOrDefault(t => t.Id == id);
        if (team == null) return NotFound();

        var user = team.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null) return NotFound();

        team.Users.Remove(user);
        return NoContent();
    }
}
