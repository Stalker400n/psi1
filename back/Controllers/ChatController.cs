using Microsoft.AspNetCore.Mvc;
using back.Models;
using back.Data;

namespace back.Controllers;

[ApiController]
[Route("teams/{id}/chat")]
public class ChatController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<ChatMessage>> GetMessages(int id)
    {
        var team = TeamStore.Teams.FirstOrDefault(t => t.Id == id);
        if (team == null)
            return NotFound(new { message = "Team not found" });

        return Ok(team.Messages.OrderBy(m => m.Timestamp));
    }

    [HttpPost]
    public ActionResult<ChatMessage> AddMessage(int id, [FromBody] ChatMessage message)
    {
        var team = TeamStore.Teams.FirstOrDefault(t => t.Id == id);
        if (team == null)
            return NotFound(new { message = "Team not found" });

        message.Id = team.Messages.Count + 1;
        message.Timestamp = DateTime.UtcNow;
        team.Messages.Add(message);

        return Ok(message);
    }

    [HttpDelete("{messageId}")]
    public IActionResult DeleteMessage(int id, int messageId)
    {
        var team = TeamStore.Teams.FirstOrDefault(t => t.Id == id);
        if (team == null)
            return NotFound(new { message = "Team not found" });

        var message = team.Messages.FirstOrDefault(m => m.Id == messageId);
        if (message == null)
            return NotFound(new { message = "Message not found" });

        team.Messages.Remove(message);
        return NoContent();
    }
}
