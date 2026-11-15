using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.GiftCardPurchasing;
using Payment.Tests.Integration.Seeds;
using Xunit;

namespace Payment.Tests.Integration.GiftCardPurchasing;

public class GiftCardPurchasingQueryTests : BaseIntegrationTest
{
    public GiftCardPurchasingQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Buyer_sees_only_the_gift_cards_they_bought_with_the_recipients_name()
    {
        var buyerId = GiftCardSeed.DemoExplorerId;
        var expectedIds = GiftCardSeed.All.Where(card => card.BuyerId == buyerId).Select(card => card.Id);
        var expected = GiftCardSeed.DemoExplorerCardForSecondDemoExplorer;
        using var client = Factory.CreateClientFor(buyerId, "explorer");

        var cards = await client.GetFromJsonAsync<List<PurchasedGiftCardDto>>(
            "/api/payment/gift-cards/purchased", JsonOptions);

        cards!.Select(card => card.Id).Should().BeEquivalentTo(expectedIds);
        cards.Should().ContainSingle(card => card.Id == expected.Id)
            .Which.Should().Be(new PurchasedGiftCardDto(
                expected.Id, expected.RecipientId, "explorer2", expected.Amount, expected.Message,
                expected.ExpiresAt, expected.StatusAt(DateTime.UtcNow)));
    }
}
