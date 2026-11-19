using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.CartGiftCard;
using Payment.Domain.GiftCards;
using Payment.Tests.Integration.Seeds;
using Xunit;

namespace Payment.Tests.Integration.CartGiftCard;

public class CartGiftCardQueryTests : BaseIntegrationTest
{
    public CartGiftCardQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task The_gift_cards_offered_for_the_cart_are_only_the_buyers_valid_unredeemed_ones()
    {
        var buyerId = ShoppingCartSeed.CartWithGiftCard.UserId;
        var now = DateTime.UtcNow;
        var expected = GiftCardSeed.All
            .Where(card => card.RecipientId == buyerId && card.StatusAt(now) == GiftCardStatus.Valid)
            .Select(card => new GiftCardForCartDto(card.Id, card.Amount, card.ExpiresAt));
        using var client = Factory.CreateClientFor(buyerId, "explorer");

        var cards = await client.GetFromJsonAsync<List<GiftCardForCartDto>>("/api/payment/cart/gift-cards", JsonOptions);

        cards.Should().BeEquivalentTo(expected);
    }
}
