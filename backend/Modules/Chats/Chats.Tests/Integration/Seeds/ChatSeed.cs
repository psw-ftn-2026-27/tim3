using Chats.Core.Chats;
using Shared.Tests;

namespace Chats.Tests.Integration.Seeds;

internal static class ChatSeed
{
    public static readonly Chat TourChatWithTwoMessages;
    public static readonly Chat OtherTourChatWithOneMessage;
    public static readonly Chat ConversationOfExplorerAndSecondExplorer;

    public static readonly Chat[] All;

    static ChatSeed()
    {
        TourChatWithTwoMessages = new Chat(
            Subject.ForTour(Guid.Parse("10000000-0000-0000-0000-000000000001")));
        OtherTourChatWithOneMessage = new Chat(
            Subject.ForTour(Guid.Parse("10000000-0000-0000-0000-000000000002")));
        ConversationOfExplorerAndSecondExplorer = new Chat(
            Subject.ForPair(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer));

        All = [TourChatWithTwoMessages, OtherTourChatWithOneMessage, ConversationOfExplorerAndSecondExplorer];
    }
}
