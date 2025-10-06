using Microsoft.AspNetCore.Mvc;
using back.Models;
using back.DataSources;

namespace back.Controllers;

[ApiController]
[Route("api/teams/{teamId}/chats")]
public class ChatsController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<ChatMessage>> GetMessages(int teamId)
    {
        var team = TeamStore.Teams.FirstOrDefault(t => t.Id == teamId);
        if (team == null)
            return NotFound(new { message = "Team not found" });

        return Ok(team.Messages.OrderBy(m => m.Timestamp));
    }

    [HttpGet("{id}")]
    public ActionResult<ChatMessage> GetMessage(int teamId, int id)
    {
        var team = TeamStore.Teams.FirstOrDefault(t => t.Id == teamId);
        if (team == null)
            return NotFound(new { message = "Team not found" });
            
        var message = team.Messages.FirstOrDefault(m => m.Id == id);
        if (message == null)
            return NotFound(new { message = "Message not found" });
            
        return Ok(message);
    }

    [HttpPost]
    public ActionResult<ChatMessage> AddMessage(int teamId, [FromBody] ChatMessage message)
    {
        var team = TeamStore.Teams.FirstOrDefault(t => t.Id == teamId);
        if (team == null)
            return NotFound(new { message = "Team not found" });

        message.Id = team.Messages.Count > 0 ? team.Messages.Max(m => m.Id) + 1 : 1;
        message.Timestamp = DateTime.UtcNow;
        team.Messages.Add(message);

        return CreatedAtAction(nameof(GetMessage), new { teamId = teamId, id = message.Id }, message);
    }

    [HttpPut("{id}")]
    public ActionResult<ChatMessage> UpdateMessage(int teamId, int id, [FromBody] ChatMessage message)
    {
        var team = TeamStore.Teams.FirstOrDefault(t => t.Id == teamId);
        if (team == null)
            return NotFound(new { message = "Team not found" });
            
        var existingMessage = team.Messages.FirstOrDefault(m => m.Id == id);
        if (existingMessage == null)
            return NotFound(new { message = "Message not found" });
            
        // Update message properties
        existingMessage.Text = message.Text;
        
        return Ok(existingMessage);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteMessage(int teamId, int id)
    {
        var team = TeamStore.Teams.FirstOrDefault(t => t.Id == teamId);
        if (team == null)
            return NotFound(new { message = "Team not found" });

        var message = team.Messages.FirstOrDefault(m => m.Id == id);
        if (message == null)
            return NotFound(new { message = "Message not found" });

        team.Messages.Remove(message);
        return NoContent();
    }
}
