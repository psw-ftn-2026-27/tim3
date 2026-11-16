using System.Net;
using System.Net.Http.Json;
using Chats.Core.ChatMessaging;
using Chats.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Chats.Tests.Integration.ChatMessaging;

public class ChatMessagingQueryTests : BaseIntegrationTest
{
    public ChatMessagingQueryTests(ChatsApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Reader_gets_the_messages_of_a_chat_newest_first_with_the_senders_name()
    {
        var chat = ChatSeed.TourChatWithTwoMessages;
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");

        var response = await client.GetAsync($"/api/chat/tours/{chat.Subject.FirstId}/messages");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PageResult<MessageDto>>(JsonOptions);
        body!.Items.Select(message => message.Id)
            .Should().BeEquivalentTo(MessageSeed.All.Where(message => message.ChatId == chat.Id).Select(message => message.Id));
        body.Items.Should().BeInDescendingOrder(message => message.SentAt);
        body.Items.Single(message => message.Id == MessageSeed.ExplorerAsksInTourChat.Id)
            .SenderName.Should().Be("explorer");
        body.Items.Single(message => message.Id == MessageSeed.SecondExplorerAnswersInTourChat.Id)
            .SenderName.Should().Be("second.explorer");
    }

    [Fact]
    public async Task Chat_that_does_not_exist_yet_has_no_messages()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync($"/api/chat/challenges/{Guid.NewGuid()}/messages");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PageResult<MessageDto>>(JsonOptions);
        body!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Member_of_a_pair_reads_their_conversation()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync(
            $"/api/chat/users/{WellKnownUsers.SecondExplorer}/{WellKnownUsers.Explorer}/messages");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PageResult<MessageDto>>(JsonOptions);
        body!.Items.Select(message => message.Id).Should().Contain(MessageSeed.ExplorerGreetsSecondExplorer.Id);
    }

    [Fact]
    public async Task User_outside_the_pair_cannot_read_their_conversation()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");

        var response = await client.GetAsync(
            $"/api/chat/users/{WellKnownUsers.Explorer}/{WellKnownUsers.SecondExplorer}/messages");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
