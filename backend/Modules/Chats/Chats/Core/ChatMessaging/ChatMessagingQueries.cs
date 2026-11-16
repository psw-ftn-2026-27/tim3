using Chats.Core.Chats;
using Chats.Core.Messages;
using Identity.Contracts;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Chats.Core.ChatMessaging;

public sealed class ChatMessagingQueries
{
    private readonly IMessageReadRepository _messageReadRepository;
    private readonly IChatReadRepository _chatReadRepository;
    private readonly ChatPrivilegesLookup _privilegesLookup;
    private readonly IIdentityApi _identityApi;

    public ChatMessagingQueries(
        IMessageReadRepository messageReadRepository,
        IChatReadRepository chatReadRepository,
        ChatPrivilegesLookup privilegesLookup,
        IIdentityApi identityApi)
    {
        _messageReadRepository = messageReadRepository;
        _chatReadRepository = chatReadRepository;
        _privilegesLookup = privilegesLookup;
        _identityApi = identityApi;
    }

    public async Task<ChatMessagesDto> GetMessagesAsync(
        Subject subject, Guid readerId, bool readerIsAdministrator, int page, int pageSize)
    {
        if (!subject.Admits(readerId))
        {
            throw new DomainException("Only the two users of a conversation can read it.");
        }

        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var rows = await _messageReadRepository.GetBySubjectAsync(subject, page, pageSize);
        var mutedUserIds = await _chatReadRepository.GetMutedUserIdsAsync(subject);
        var privileges = await _privilegesLookup.GetAsync(subject, readerId, readerIsAdministrator);
        var readerIsPrivileged = privileges.Includes(readerId);
        var shownMutedUserIds = readerIsPrivileged ? mutedUserIds : [];

        var userIds = rows.Items.Select(row => row.SenderId).Concat(shownMutedUserIds).Distinct().ToList();
        var names = await _identityApi.GetDisplayNamesAsync(userIds);
        var namesById = names.ToDictionary(name => name.UserId, name => name.DisplayName);

        var items = rows.Items
            .Select(row => new MessageDto(
                row.Id,
                row.SenderId,
                namesById.GetValueOrDefault(row.SenderId, ""),
                row.Text,
                row.SentAt,
                privileges.Includes(row.SenderId)))
            .ToList();
        var mutedUsers = shownMutedUserIds
            .Select(userId => new MutedUserDto(userId, namesById.GetValueOrDefault(userId, "")))
            .ToList();

        return new ChatMessagesDto(
            new PageResult<MessageDto>(items, rows.TotalCount),
            readerIsPrivileged,
            mutedUserIds.Contains(readerId),
            mutedUsers);
    }
}
