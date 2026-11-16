namespace Chats.Core.Messages;

public interface IMessageRepository
{
    Task<Message?> GetByIdAsync(Guid id);
    void Add(Message message);
    void Remove(Message message);
}
