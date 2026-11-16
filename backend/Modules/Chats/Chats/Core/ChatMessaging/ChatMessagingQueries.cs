using Chats.Core.Chats;
using Chats.Core.Messages;
using Identity.Contracts;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Chats.Core.ChatMessaging;

public sealed class ChatMessagingQueries
{
    private readonly IMessageReadRepository _messageReadRepository;
    private readonly IIdentityApi _identityApi;

    public ChatMessagingQueries(IMessageReadRepository messageReadRepository, IIdentityApi identityApi)
    {
        _messageReadRepository = messageReadRepository;
        _identityApi = identityApi;
    }

    public async Task<PageResult<MessageDto>> GetMessagesAsync(Subject subject, Guid readerId, int page, int pageSize)
    {
        if (!subject.Admits(readerId))
        {
            throw new DomainException("Only the two users of a conversation can read it.");
        }

        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var rows = await _messageReadRepository.GetBySubjectAsync(subject, page, pageSize);
        var senderIds = rows.Items.Select(row => row.SenderId).Distinct().ToList();
        var names = await _identityApi.GetDisplayNamesAsync(senderIds);
        var namesById = names.ToDictionary(name => name.UserId, name => name.DisplayName);

        var items = rows.Items
            .Select(row => new MessageDto(
                row.Id, row.SenderId, namesById.GetValueOrDefault(row.SenderId, ""), row.Text, row.SentAt))
            .ToList();
        return new PageResult<MessageDto>(items, rows.TotalCount);
    }
}
