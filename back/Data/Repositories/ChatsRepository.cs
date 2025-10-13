using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using back.Models;
using Microsoft.EntityFrameworkCore;

namespace back.Data.Repositories;

public class ChatsRepository : IChatsRepository
{
    private readonly ApplicationDbContext _context;

    public ChatsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ChatMessage>?> GetMessagesAsync(int teamId)
    {
        var team = await _context.Teams
            .Include(t => t.Messages)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null) return null;

        return team.Messages.OrderBy(m => m.Timestamp).ToList();
    }

    public async Task<ChatMessage?> GetMessageAsync(int teamId, int messageId)
    {
        var team = await _context.Teams
            .Include(t => t.Messages)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null) return null;

        return team.Messages.FirstOrDefault(m => m.Id == messageId);
    }

    public async Task<ChatMessage?> AddMessageAsync(int teamId, ChatMessage message)
    {
        var team = await _context.Teams
            .Include(t => t.Messages)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null) return null;

        message.Timestamp = DateTime.UtcNow;

        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();

        team.Messages.Add(message);
        await _context.SaveChangesAsync();

        return message;
    }

    public async Task<ChatMessage?> UpdateMessageAsync(int teamId, int messageId, ChatMessage message)
    {
        var team = await _context.Teams
            .Include(t => t.Messages)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null) return null;

        var existingMessage = team.Messages.FirstOrDefault(m => m.Id == messageId);
        if (existingMessage == null) return null;

        existingMessage.Text = message.Text;

        _context.Entry(existingMessage).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return existingMessage;
    }

    public async Task<bool> DeleteMessageAsync(int teamId, int messageId)
    {
        var team = await _context.Teams
            .Include(t => t.Messages)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null) return false;

        var message = team.Messages.FirstOrDefault(m => m.Id == messageId);
        if (message == null) return false;

        team.Messages.Remove(message);
        await _context.SaveChangesAsync();

        return true;
    }
}
