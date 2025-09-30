using Microsoft.AspNetCore.Mvc;
using back.Models;
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

  [HttpGet("{id}/songs")]
  public ActionResult<IEnumerable<Song>> GetSongs(int id)
  {
    var team = Teams.FirstOrDefault(t => t.Id == id);
    if (team == null) return NotFound();
    return Ok(team.Songs);
  }

  [HttpPost("{id}/songs")]
  public ActionResult<Song> AddSong(int id, [FromBody] Song song)
  {
    var team = Teams.FirstOrDefault(t => t.Id == id);
    if (team == null) return NotFound();

    song.Id = team.Songs.Count + 1;
    team.Songs.Add(song);
    return Ok(song);
  }

  [HttpDelete("{id}/songs/{songId}")]
  public IActionResult DeleteSong(int id, int songId)
  {
    var team = Teams.FirstOrDefault(t => t.Id == id);
    if (team == null) return NotFound();

    var song = team.Songs.FirstOrDefault(s => s.Id == songId);
    if (song == null) return NotFound();

    team.Songs.Remove(song);
    return NoContent();
  }

  [HttpGet("{id}/users")]
  public ActionResult<IEnumerable<User>> GetUsers(int id)
  {
    var team = Teams.FirstOrDefault(t => t.Id == id);
    if (team == null) return NotFound();
    return Ok(team.Users);
  }

  [HttpPost("{id}/users")]
  public ActionResult<User> AddUser(int id, [FromBody] User user)
  {
    var team = Teams.FirstOrDefault(t => t.Id == id);
    if (team == null) return NotFound();

    user.Id = team.Users.Count + 1;
    team.Users.Add(user);
    return Ok(user);
  }

  [HttpDelete("{id}/users/{userId}")]
  public IActionResult DeleteUser(int id, int userId)
  {
    var team = Teams.FirstOrDefault(t => t.Id == id);
    if (team == null) return NotFound();

    var user = team.Users.FirstOrDefault(u => u.Id == userId);
    if (user == null) return NotFound();

    team.Users.Remove(user);
    return NoContent();
  }
}
