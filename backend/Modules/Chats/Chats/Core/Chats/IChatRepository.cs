namespace Chats.Core.Chats;

public interface IChatRepository
{
    Task<Chat?> GetBySubjectAsync(Subject subject);
    void Add(Chat chat);
}
