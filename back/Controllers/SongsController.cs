using Microsoft.AspNetCore.Mvc;
using back.Models;
using back.DataSources;

namespace back.Controllers;

[ApiController]
[Route("api/teams/{teamId}/songs")]
public class SongsController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Song>> GetSongs(int teamId)
    {
        var team = TeamStore.Teams.FirstOrDefault(t => t.Id == teamId);
        if (team == null) return NotFound(new { message = "Team not found" });
        return Ok(team.Songs);
    }

    [HttpGet("{id}")]
    public ActionResult<Song> GetSong(int teamId, int id)
    {
        var team = TeamStore.Teams.FirstOrDefault(t => t.Id == teamId);
        if (team == null) return NotFound(new { message = "Team not found" });
        
        var song = team.Songs.FirstOrDefault(s => s.Id == id);
        if (song == null) return NotFound(new { message = "Song not found" });
        
        return Ok(song);
    }

    [HttpPost]
    public ActionResult<Song> AddSong(int teamId, [FromBody] Song song)
    {
        var team = TeamStore.Teams.FirstOrDefault(t => t.Id == teamId);
        if (team == null) return NotFound(new { message = "Team not found" });

        song.Id = team.Songs.Count > 0 ? team.Songs.Max(s => s.Id) + 1 : 1;
        team.Songs.Add(song);
        
        return CreatedAtAction(nameof(GetSong), new { teamId = teamId, id = song.Id }, song);
    }

    [HttpPut("{id}")]
    public ActionResult<Song> UpdateSong(int teamId, int id, [FromBody] Song song)
    {
        var team = TeamStore.Teams.FirstOrDefault(t => t.Id == teamId);
        if (team == null) return NotFound(new { message = "Team not found" });
        
        var existingSong = team.Songs.FirstOrDefault(s => s.Id == id);
        if (existingSong == null) return NotFound(new { message = "Song not found" });
        
        // Update song properties
        existingSong.Link = song.Link;
        
        return Ok(existingSong);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteSong(int teamId, int id)
    {
        var team = TeamStore.Teams.FirstOrDefault(t => t.Id == teamId);
        if (team == null) return NotFound(new { message = "Team not found" });

        var song = team.Songs.FirstOrDefault(s => s.Id == id);
        if (song == null) return NotFound(new { message = "Song not found" });

        team.Songs.Remove(song);
        return NoContent();
    }
}
