using Microsoft.AspNetCore.Mvc;
using back.Models;

namespace back.Controllers;

[ApiController]
[Route("teams")]
public class TeamController : ControllerBase
{
  private static readonly List<Team> Teams = new();

  [HttpGet]
  public ActionResult<IEnumerable<Team>> GetTeams()
  {
    return Ok(Teams);
  }

  [HttpPost]
  public ActionResult<Team> CreateTeam([FromBody] Team team)
  {
    team.Id = Teams.Count + 1;
    Teams.Add(team);
    return CreatedAtAction(nameof(GetTeams), new { id = team.Id }, team);
  }
}
