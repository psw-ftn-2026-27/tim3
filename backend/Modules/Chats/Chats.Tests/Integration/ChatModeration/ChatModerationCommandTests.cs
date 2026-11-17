using System.Net;
using Chats.Infrastructure.Persistence;
using Chats.Tests.Integration.Seeds;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shared.Tests;
using Xunit;

namespace Chats.Tests.Integration.ChatModeration;

public class ChatModerationCommandTests : BaseIntegrationTest
{
    public ChatModerationCommandTests(ChatsApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Tour_author_deletes_someone_elses_message_and_it_disappears()
    {
        var chat = ChatSeed.FortressWalkChatWithMutedThirdExplorer;
        var message = MessageSeed.SecondExplorerWritesInFortressWalkChat;
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync($"/api/chat/tours/{chat.Subject.FirstId}/messages/{message.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<ChatsDbContext>();
        (await context.Messages.AnyAsync(stored => stored.Id == message.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task User_who_is_not_privileged_cannot_delete_a_message_and_it_remains()
    {
        var chat = ChatSeed.TourChatWithTwoMessages;
        var message = MessageSeed.ExplorerAsksInTourChat;
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");

        var response = await client.DeleteAsync($"/api/chat/tours/{chat.Subject.FirstId}/messages/{message.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<ChatsDbContext>();
        (await context.Messages.AnyAsync(stored => stored.Id == message.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task Message_of_another_chat_cannot_be_deleted_through_this_chat()
    {
        var chat = ChatSeed.ChallengeChatWithMutedSecondExplorer;
        var messageOfAnotherChat = MessageSeed.ExplorerAsksInTourChat;
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync(
            $"/api/chat/challenges/{chat.Subject.FirstId}/messages/{messageOfAnotherChat.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var context = Factory.CreateContext<ChatsDbContext>();
        (await context.Messages.AnyAsync(stored => stored.Id == messageOfAnotherChat.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task Administrator_mutes_a_user_in_a_challenge_chat_and_the_user_is_among_the_muted()
    {
        var chat = ChatSeed.ChallengeChatWithMutedSecondExplorer;
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync(
            $"/api/chat/challenges/{chat.Subject.FirstId}/muted-users/{WellKnownUsers.Explorer}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<ChatsDbContext>();
        var stored = await context.Chats.SingleAsync(stored => stored.Id == chat.Id);
        stored.MutedUserIds.Should().Contain(WellKnownUsers.Explorer);
    }

    [Fact]
    public async Task Tour_author_unmutes_a_user_and_the_user_is_no_longer_muted()
    {
        var chat = ChatSeed.FortressWalkChatWithMutedThirdExplorer;
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync(
            $"/api/chat/tours/{chat.Subject.FirstId}/muted-users/{WellKnownUsers.ThirdExplorer}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<ChatsDbContext>();
        var stored = await context.Chats.SingleAsync(stored => stored.Id == chat.Id);
        stored.MutedUserIds.Should().NotContain(WellKnownUsers.ThirdExplorer);
    }

    [Fact]
    public async Task Anonymous_user_cannot_delete_a_message()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync(
            $"/api/chat/tours/{ChatSeed.TourChatWithTwoMessages.Subject.FirstId}/messages/{MessageSeed.ExplorerAsksInTourChat.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Anonymous_user_cannot_mute_a_user()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync(
            $"/api/chat/challenges/{ChatSeed.ChallengeChatWithMutedSecondExplorer.Subject.FirstId}/muted-users/{WellKnownUsers.Explorer}",
            null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Anonymous_user_cannot_unmute_a_user()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync(
            $"/api/chat/challenges/{ChatSeed.ChallengeChatWithMutedSecondExplorer.Subject.FirstId}/muted-users/{WellKnownUsers.SecondExplorer}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
