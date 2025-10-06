using Microsoft.AspNetCore.Mvc;
using back.Models;
using back.DataSources;
using System.Security.Cryptography;

namespace back.Controllers;

[ApiController]
[Route("api/teams")]
public class TeamsController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Team>> GetTeams()
    {
        return Ok(TeamStore.Teams);
    }

    [HttpGet("{id}")]
    public ActionResult<Team> GetTeam(int id)
    {
        var team = TeamStore.Teams.FirstOrDefault(t => t.Id == id);
        if (team == null)
        {
            return NotFound(new { message = "Team not found" });
        }
        return Ok(team);
    }

    [HttpPost]
    public ActionResult<Team> CreateTeam([FromBody] Team team)
    {
        int newId;
        do
        {
            newId = RandomNumberGenerator.GetInt32(100000, 1000000);
        }
        while (TeamStore.Teams.Any(t => t.Id == newId));

        team.Id = newId;
        TeamStore.Teams.Add(team);

        return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, team);
    }

    [HttpPut("{id}")]
    public ActionResult<Team> UpdateTeam(int id, [FromBody] Team team)
    {
        var currentTeam = TeamStore.Teams.FirstOrDefault(t => t.Id == id);
        if (currentTeam == null)
        {
            return NotFound(new { message = "Team not found" });
        }

        currentTeam.Name = team.Name;
        currentTeam.IsPrivate = team.IsPrivate;

        return Ok(currentTeam);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteTeam(int id)
    {
        var team = TeamStore.Teams.FirstOrDefault(t => t.Id == id);
        if (team == null)
        {
            return NotFound(new { message = "Team not found" });
        }

        TeamStore.Teams.Remove(team);
        return NoContent();
    }
}
