using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.GiftCardReceiving;
using Payment.Tests.Integration.Seeds;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.GiftCardReceiving;

public class GiftCardReceivingQueryTests : BaseIntegrationTest
{
    public GiftCardReceivingQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Recipient_sees_only_the_gift_cards_they_received_with_the_senders_name()
    {
        var recipientId = GiftCardSeed.DemoExplorerId;
        var expectedIds = GiftCardSeed.All.Where(card => card.RecipientId == recipientId).Select(card => card.Id).ToList();
        var expected = GiftCardSeed.SecondDemoExplorerCardForDemoExplorer;
        using var client = Factory.CreateClientFor(recipientId, "explorer");

        var cards = await client.GetFromJsonAsync<PageResult<ReceivedGiftCardDto>>(
            "/api/payment/gift-cards/received?page=1&pageSize=100", JsonOptions);

        cards!.Items.Select(card => card.Id).Should().BeEquivalentTo(expectedIds);
        cards.TotalCount.Should().Be(expectedIds.Count);
        cards.Items.Should().ContainSingle(card => card.Id == expected.Id)
            .Which.Should().Be(new ReceivedGiftCardDto(
                expected.Id, expected.BuyerId, "explorer2", expected.Amount, expected.Message,
                expected.ExpiresAt, expected.StatusAt(DateTime.UtcNow)));
    }

    [Fact]
    public async Task Recipient_pages_through_received_gift_cards_newest_first()
    {
        var recipientId = GiftCardSeed.DemoExplorerId;
        var received = GiftCardSeed.All.Where(card => card.RecipientId == recipientId).ToList();
        var secondNewest = received.OrderByDescending(card => card.PurchasedAt).ElementAt(1);
        using var client = Factory.CreateClientFor(recipientId, "explorer");

        var cards = await client.GetFromJsonAsync<PageResult<ReceivedGiftCardDto>>(
            "/api/payment/gift-cards/received?page=2&pageSize=1", JsonOptions);

        cards!.Items.Should().ContainSingle().Which.Id.Should().Be(secondNewest.Id);
        cards.TotalCount.Should().Be(received.Count);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("administrator", HttpStatusCode.Forbidden)]
    public async Task Only_explorers_can_see_received_gift_cards(string? role, HttpStatusCode expected)
    {
        using var client = role is null
            ? Factory.CreateClient()
            : Factory.CreateClientFor(WellKnownUsers.Administrator, role);

        var response = await client.GetAsync("/api/payment/gift-cards/received");

        response.StatusCode.Should().Be(expected);
    }
}
