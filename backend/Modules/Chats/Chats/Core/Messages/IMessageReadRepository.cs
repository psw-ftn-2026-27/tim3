using Chats.Core.ChatMessaging;
using Chats.Core.Chats;
using Shared.Domain;

namespace Chats.Core.Messages;

public interface IMessageReadRepository
{
    Task<PageResult<MessageRowDto>> GetBySubjectAsync(Subject subject, int page, int pageSize);
}
