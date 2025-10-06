using Microsoft.AspNetCore.Mvc;
using back.Models;
using back.Data;
using System.Security.Cryptography;

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
    int newId;
    do
    {
      newId = RandomNumberGenerator.GetInt32(100000, 1000000);
    }
    while (Teams.Any(t => t.Id == newId));

    team.Id = newId;
    Teams.Add(team);

    return CreatedAtAction(nameof(GetTeams), new { id = team.Id }, team);
  }

  [HttpGet("{id}")]
  public ActionResult<Team> GetTeamById(int id)
  {
    var team = Teams.FirstOrDefault(t => t.Id == id);
    if (team == null)
    {
      return NotFound();
    }
    return Ok(team);
  }

  [HttpPut("{id}")]
  public ActionResult<Team> EditTeamSettings(int id, [FromBody] Team team)
  {
    var currentTeam = Teams.FirstOrDefault(t => t.Id == id);
    if (currentTeam == null)
    {
      return NotFound();
    }

    currentTeam.Name = team.Name;
    currentTeam.IsPrivate = team.IsPrivate;

    return Ok(currentTeam);
  }

}
