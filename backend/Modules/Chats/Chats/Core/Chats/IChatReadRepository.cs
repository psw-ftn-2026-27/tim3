namespace Chats.Core.Chats;

public interface IChatReadRepository
{
    // Empty when the chat does not exist yet.
    Task<List<Guid>> GetMutedUserIdsAsync(Subject subject);
}
