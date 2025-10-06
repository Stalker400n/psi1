using Microsoft.AspNetCore.Mvc;
using back.Models;
using back.Data;
using Microsoft.EntityFrameworkCore;

namespace back.Controllers;

[ApiController]
[Route("teams/{teamId}/chats")]
public class ChatsController : ControllerBase
{
  private readonly ApplicationDbContext _context;

  public ChatsController(ApplicationDbContext context)
  {
    _context = context;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<ChatMessage>>> GetMessages(int teamId)
  {
    var team = await _context.Teams
        .Include(t => t.Messages)
        .FirstOrDefaultAsync(t => t.Id == teamId);

    if (team == null)
      return NotFound(new { message = "Team not found" });

    return Ok(team.Messages.OrderBy(m => m.Timestamp));
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<ChatMessage>> GetMessage(int teamId, int id)
  {
    var team = await _context.Teams
        .Include(t => t.Messages)
        .FirstOrDefaultAsync(t => t.Id == teamId);

    if (team == null)
      return NotFound(new { message = "Team not found" });

    var message = team.Messages.FirstOrDefault(m => m.Id == id);
    if (message == null)
      return NotFound(new { message = "Message not found" });

    return Ok(message);
  }

  [HttpPost]
  public async Task<ActionResult<ChatMessage>> AddMessage(int teamId, [FromBody] ChatMessage message)
  {
    var team = await _context.Teams
        .Include(t => t.Messages)
        .FirstOrDefaultAsync(t => t.Id == teamId);

    if (team == null)
      return NotFound(new { message = "Team not found" });

    message.Timestamp = DateTime.UtcNow;
    
    _context.ChatMessages.Add(message);
    await _context.SaveChangesAsync();
    
    team.Messages.Add(message);
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(GetMessage), new { teamId = teamId, id = message.Id }, message);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<ChatMessage>> UpdateMessage(int teamId, int id, [FromBody] ChatMessage message)
  {
    var team = await _context.Teams
        .Include(t => t.Messages)
        .FirstOrDefaultAsync(t => t.Id == teamId);

    if (team == null)
      return NotFound(new { message = "Team not found" });

    var existingMessage = team.Messages.FirstOrDefault(m => m.Id == id);
    if (existingMessage == null)
      return NotFound(new { message = "Message not found" });

    existingMessage.Text = message.Text;

    _context.Entry(existingMessage).State = EntityState.Modified;
    await _context.SaveChangesAsync();

    return Ok(existingMessage);
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteMessage(int teamId, int id)
  {
    var team = await _context.Teams
        .Include(t => t.Messages)
        .FirstOrDefaultAsync(t => t.Id == teamId);

    if (team == null)
      return NotFound(new { message = "Team not found" });

    var message = team.Messages.FirstOrDefault(m => m.Id == id);
    if (message == null)
      return NotFound(new { message = "Message not found" });

    team.Messages.Remove(message);
    await _context.SaveChangesAsync();

    return NoContent();
  }
}
