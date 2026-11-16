using Chats.Core.Messages;
using Shared.Tests;

namespace Chats.Tests.Integration.Seeds;

internal static class MessageSeed
{
    public static readonly Message ExplorerAsksInTourChat;
    public static readonly Message SecondExplorerAnswersInTourChat;
    public static readonly Message ThirdExplorerWritesInOtherTourChat;
    public static readonly Message ExplorerGreetsSecondExplorer;

    public static readonly Message[] All;

    static MessageSeed()
    {
        ExplorerAsksInTourChat = ChatSeed.TourChatWithTwoMessages
            .Write(WellKnownUsers.Explorer, "Is this tour suitable for children?");
        SecondExplorerAnswersInTourChat = ChatSeed.TourChatWithTwoMessages
            .Write(WellKnownUsers.SecondExplorer, "Yes, the walk is short and flat.");
        ThirdExplorerWritesInOtherTourChat = ChatSeed.OtherTourChatWithOneMessage
            .Write(WellKnownUsers.ThirdExplorer, "Where does the tour start?");
        ExplorerGreetsSecondExplorer = ChatSeed.ConversationOfExplorerAndSecondExplorer
            .Write(WellKnownUsers.Explorer, "Hi, did you finish the fortress tour?");

        All = [ExplorerAsksInTourChat, SecondExplorerAnswersInTourChat, ThirdExplorerWritesInOtherTourChat, ExplorerGreetsSecondExplorer];
    }
}
