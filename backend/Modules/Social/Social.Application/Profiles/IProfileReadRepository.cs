using Shared.Domain;
using Social.Application.FollowRequesting;

namespace Social.Application.Profiles;

public interface IProfileReadRepository
{
    Task<PageResult<SentFollowRequestRowDto>> GetSentRequestsAsync(Guid senderId, int page, int pageSize);
    Task<PageResult<IncomingFollowRequestRowDto>> GetIncomingRequestsAsync(Guid recipientId, int page, int pageSize);
}
