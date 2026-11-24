using Social.Domain.Profiles;

namespace Social.Application.Profiles;

public interface IProfileRepository
{
    Task<Profile?> GetByUserIdAsync(Guid userId);
    Task<Profile?> GetBySentRequestIdAsync(Guid requestId);
    void Add(Profile profile);
}
