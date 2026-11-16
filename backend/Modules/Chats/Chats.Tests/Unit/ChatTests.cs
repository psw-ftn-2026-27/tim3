using Chats.Core.Chats;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Chats.Tests.Unit;

public class ChatTests
{
    private static readonly Guid Moderator = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid Participant = Guid.Parse("00000000-0000-0000-0000-000000000002");

    private static Privileges ModeratorOnly() => new([Moderator]);

    private static Chat TourChatWithMutedParticipant()
    {
        var chat = new Chat(Subject.ForTour(Guid.NewGuid()));
        chat.Mute(Moderator, Participant, ModeratorOnly());
        return chat;
    }

    [Fact]
    public void Muted_user_cannot_write_in_that_chat()
    {
        var chat = TourChatWithMutedParticipant();

        var writing = () => chat.Write(Participant, "Hello again.");

        writing.Should().Throw<DomainException>();
    }

    [Fact]
    public void User_muted_in_one_chat_still_writes_in_another()
    {
        var chatWhereMuted = TourChatWithMutedParticipant();
        var otherChat = new Chat(Subject.ForTour(Guid.NewGuid()));

        var message = otherChat.Write(Participant, "Hello here.");

        message.SenderId.Should().Be(Participant);
    }

    [Fact]
    public void User_writes_again_after_being_unmuted()
    {
        var chat = TourChatWithMutedParticipant();
        chat.Unmute(Moderator, Participant, ModeratorOnly());

        var message = chat.Write(Participant, "Thanks, I will behave.");

        message.SenderId.Should().Be(Participant);
    }
}
