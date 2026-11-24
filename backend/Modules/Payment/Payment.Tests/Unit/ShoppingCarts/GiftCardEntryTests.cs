using FluentAssertions;
using Payment.Domain.GiftCards;
using Payment.Domain.ShoppingCarts;
using Shared.Domain.Exceptions;
using Xunit;

namespace Payment.Tests.Unit.ShoppingCarts;

public class GiftCardEntryTests
{
    private static readonly DateTime Now = new(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid Buyer = Guid.NewGuid();

    [Theory]
    [InlineData("other recipient")]
    [InlineData("expired")]
    [InlineData("redeemed")]
    public void A_gift_card_the_buyer_cannot_use_is_not_chosen(string problem)
    {
        var cart = new ShoppingCart(Buyer);
        var giftCard = CreateGiftCard(problem == "other recipient" ? Guid.NewGuid() : Buyer);
        if (problem == "redeemed")
        {
            giftCard.Redeem(Buyer, Now);
        }
        var moment = problem == "expired" ? giftCard.ExpiresAt : Now;

        var enter = () => CreatePurchasing().EnterGiftCard(cart, giftCard, moment);

        enter.Should().Throw<DomainException>();
        cart.GiftCardId.Should().BeNull();
    }

    [Fact]
    public void A_second_gift_card_cannot_be_chosen_until_the_first_is_removed()
    {
        var cart = new ShoppingCart(Buyer);
        var first = CreateGiftCard(Buyer);
        CreatePurchasing().EnterGiftCard(cart, first, Now);

        var enter = () => CreatePurchasing().EnterGiftCard(cart, CreateGiftCard(Buyer), Now);

        enter.Should().Throw<DomainException>();
        cart.GiftCardId.Should().Be(first.Id);
    }

    private static CartPurchasing CreatePurchasing()
    {
        return new CartPurchasing(new CartPricing(new CouponAllocation()));
    }

    private static GiftCard CreateGiftCard(Guid recipientId)
    {
        return new GiftCard(Guid.NewGuid(), recipientId, 20, null, Now.AddDays(-1));
    }
}
