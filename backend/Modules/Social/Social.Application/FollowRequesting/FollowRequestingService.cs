using Identity.Contracts;
using Shared.Domain.Exceptions;
using Social.Application.Profiles;
using Social.Domain.Profiles;

namespace Social.Application.FollowRequesting;

public sealed class FollowRequestingService
{
    private readonly IProfileRepository _profileRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityApi _identityApi;

    public FollowRequestingService(IProfileRepository profileRepository, IUnitOfWork unitOfWork, IIdentityApi identityApi)
    {
        _profileRepository = profileRepository;
        _unitOfWork = unitOfWork;
        _identityApi = identityApi;
    }

    public async Task SendAsync(Guid userId, SendFollowRequestDto dto)
    {
        if (!await _identityApi.ExistsAsync(dto.RecipientId))
        {
            throw new NotFoundException($"User {dto.RecipientId} does not exist.");
        }

        var sender = await _profileRepository.GetByUserIdAsync(userId);
        if (sender is null)
        {
            sender = new Profile(userId);
            _profileRepository.Add(sender);
        }
        var recipient = await _profileRepository.GetByUserIdAsync(dto.RecipientId) ?? new Profile(dto.RecipientId);

        FollowRequestSender.Send(sender, recipient, DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task CancelAsync(Guid userId, Guid requestId)
    {
        var sender = await _profileRepository.GetByUserIdAsync(userId);
        if (sender is null)
        {
            throw new NotFoundException($"Follow request {requestId} does not exist.");
        }

        sender.CancelFollowRequest(requestId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AcceptAsync(Guid userId, Guid requestId)
    {
        var sender = await GetSenderAsync(requestId);

        sender.AcceptFollowRequest(requestId, userId, DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RejectAsync(Guid userId, Guid requestId)
    {
        var sender = await GetSenderAsync(requestId);

        sender.RejectFollowRequest(requestId, userId, DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Profile> GetSenderAsync(Guid requestId)
    {
        var sender = await _profileRepository.GetBySentRequestIdAsync(requestId);
        if (sender is null)
        {
            throw new NotFoundException($"Follow request {requestId} does not exist.");
        }
        return sender;
    }
}
