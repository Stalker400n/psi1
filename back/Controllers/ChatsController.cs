using Microsoft.AspNetCore.Mvc;
using back.Models;
using back.Data.Repositories;

namespace back.Controllers;

[ApiController]
[Route("teams/{teamId}/chats")]
public class ChatsController : ControllerBase
{
  private readonly IChatsRepository _chatsRepository;

  public ChatsController(IChatsRepository chatsRepository)
  {
    if (chatsRepository == null) throw new ArgumentNullException(nameof(chatsRepository));
    _chatsRepository = chatsRepository;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<ChatMessage>>> GetMessages(int teamId)
  {
    var messages = await _chatsRepository.GetMessagesAsync(teamId);
    if (messages == null) return NotFound(new { message = "Team not found" });
    return Ok(messages);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<ChatMessage>> GetMessage(int teamId, int id)
  {
    var message = await _chatsRepository.GetMessageAsync(teamId, id);
    if (message == null) return NotFound(new { message = "Team or message not found" });
    return Ok(message);
  }

  [HttpPost]
  public async Task<ActionResult<ChatMessage>> AddMessage(int teamId, [FromBody] ChatMessage message)
  {
    var created = await _chatsRepository.AddMessageAsync(teamId, message);
    if (created == null) return NotFound(new { message = "Team not found" });
    return CreatedAtAction(nameof(GetMessage), new { teamId = teamId, id = created.Id }, created);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<ChatMessage>> UpdateMessage(int teamId, int id, [FromBody] ChatMessage message)
  {
    var updated = await _chatsRepository.UpdateMessageAsync(teamId, id, message);
    if (updated == null) return NotFound(new { message = "Team or message not found" });
    return Ok(updated);
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteMessage(int teamId, int id)
  {
    var deleted = await _chatsRepository.DeleteMessageAsync(teamId, id);
    if (!deleted) return NotFound(new { message = "Team or message not found" });
    return NoContent();
  }
}
