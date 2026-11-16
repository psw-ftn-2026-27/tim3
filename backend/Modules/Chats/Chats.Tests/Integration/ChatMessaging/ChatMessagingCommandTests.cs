using System.Net;
using System.Net.Http.Json;
using Chats.Core.ChatMessaging;
using Chats.Core.Chats;
using Chats.Infrastructure.Persistence;
using Chats.Tests.Integration.Seeds;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shared.Tests;
using Xunit;

namespace Chats.Tests.Integration.ChatMessaging;

public class ChatMessagingCommandTests : BaseIntegrationTest
{
    public ChatMessagingCommandTests(ChatsApiFactory factory) : base(factory) { }

    [Fact]
    public async Task First_message_in_a_tour_chat_creates_the_chat_and_stores_the_message()
    {
        var tourId = Guid.NewGuid();
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new SendMessageDto("Has anyone done this tour in winter?");

        var response = await client.PostAsJsonAsync($"/api/chat/tours/{tourId}/messages", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<ChatsDbContext>();
        var chat = await context.Chats.SingleAsync(chat =>
            chat.Subject.Kind == SubjectKind.Tour && chat.Subject.FirstId == tourId);
        var message = await context.Messages.SingleAsync(message => message.ChatId == chat.Id);
        message.SenderId.Should().Be(WellKnownUsers.Explorer);
        message.Text.Should().Be(request.Text);
    }

    [Fact]
    public async Task Message_in_a_chat_that_already_exists_does_not_create_a_new_chat()
    {
        var existing = ChatSeed.TourChatWithTwoMessages;
        int chatCount;
        using (var before = Factory.CreateContext<ChatsDbContext>())
        {
            chatCount = await before.Chats.CountAsync();
        }
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");
        var request = new SendMessageDto("I am joining this tour next week.");

        var response = await client.PostAsJsonAsync($"/api/chat/tours/{existing.Subject.FirstId}/messages", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<ChatsDbContext>();
        (await context.Chats.CountAsync()).Should().Be(chatCount);
        (await context.Messages.AnyAsync(message => message.ChatId == existing.Id && message.Text == request.Text))
            .Should().BeTrue();
    }

    [Fact]
    public async Task User_outside_the_pair_cannot_send_a_message_and_nothing_is_stored()
    {
        var conversation = ChatSeed.ConversationOfExplorerAndSecondExplorer;
        int messageCount;
        using (var before = Factory.CreateContext<ChatsDbContext>())
        {
            messageCount = await before.Messages.CountAsync();
        }
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");

        var response = await client.PostAsJsonAsync(
            $"/api/chat/users/{WellKnownUsers.Explorer}/{WellKnownUsers.SecondExplorer}/messages",
            new SendMessageDto("Can I join your conversation?"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<ChatsDbContext>();
        (await context.Messages.CountAsync()).Should().Be(messageCount);
        (await context.Messages.AnyAsync(message =>
            message.ChatId == conversation.Id && message.SenderId == WellKnownUsers.ThirdExplorer))
            .Should().BeFalse();
    }

    [Fact]
    public async Task Anonymous_user_cannot_send_a_message()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/chat/tours/{ChatSeed.TourChatWithTwoMessages.Subject.FirstId}/messages",
            new SendMessageDto("Hello?"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
