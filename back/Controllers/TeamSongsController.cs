using Microsoft.AspNetCore.Mvc;
using back.Models;
using back.Data;

namespace back.Controllers;

[ApiController]
[Route("teams/{id}/songs")]
public class TeamSongsController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Song>> GetSongs(int id)
    {
        var team = TeamStore.Teams.FirstOrDefault(t => t.Id == id);
        if (team == null) return NotFound();
        return Ok(team.Songs);
    }

    [HttpPost]
    public ActionResult<Song> AddSong(int id, [FromBody] Song song)
    {
        var team = TeamStore.Teams.FirstOrDefault(t => t.Id == id);
        if (team == null) return NotFound();

        song.Id = team.Songs.Count + 1;
        team.Songs.Add(song);
        return Ok(song);
    }

    [HttpDelete("{songId}")]
    public IActionResult DeleteSong(int id, int songId)
    {
        var team = TeamStore.Teams.FirstOrDefault(t => t.Id == id);
        if (team == null) return NotFound();

        var song = team.Songs.FirstOrDefault(s => s.Id == songId);
        if (song == null) return NotFound();

        team.Songs.Remove(song);
        return NoContent();
    }
}
