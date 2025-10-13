using back.Models;

namespace back.Data.Repositories;

public interface IChatsRepository
{
    Task<IEnumerable<ChatMessage>> GetMessagesAsync(int teamId);

    Task<ChatMessage?> GetMessageAsync(int teamId, int messageId);

    Task<ChatMessage> AddMessageAsync(int teamId, ChatMessage message);

    Task<ChatMessage?> UpdateMessageAsync(int teamId, int messageId, ChatMessage message);

    Task<bool> DeleteMessageAsync(int teamId, int messageId);
}