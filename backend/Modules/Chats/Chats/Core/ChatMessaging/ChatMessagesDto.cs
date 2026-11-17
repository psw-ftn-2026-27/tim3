using Shared.Domain;

namespace Chats.Core.ChatMessaging;

// One page of a chat's messages together with the viewer's standing in that chat.
// MutedUsers is filled only for a privileged viewer and is empty for everyone else.
public sealed record ChatMessagesDto(
    PageResult<MessageDto> Messages,
    bool ViewerIsPrivileged,
    bool ViewerIsMuted,
    List<MutedUserDto> MutedUsers);
