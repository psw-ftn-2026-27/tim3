using FluentAssertions;
using Payment.Domain.GiftCards;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.GiftCards;

public class GiftCardTests
{
    private static readonly DateTime PurchaseTime = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(9)]
    [InlineData(1001)]
    public void A_card_with_an_amount_outside_ten_to_one_thousand_ec_is_rejected(int amount)
    {
        var create = () => new GiftCard(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer, amount, null, PurchaseTime);

        create.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(10)]
    [InlineData(1000)]
    public void A_card_with_an_amount_at_the_limits_is_accepted(int amount)
    {
        var giftCard = new GiftCard(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer, amount, null, PurchaseTime);

        giftCard.Amount.Should().Be(amount);
    }

    [Fact]
    public void A_card_with_a_message_longer_than_two_hundred_characters_is_rejected()
    {
        var message = new string('a', 201);

        var create = () => new GiftCard(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer, 50, message, PurchaseTime);

        create.Should().Throw<DomainException>();
    }

    [Fact]
    public void A_buyer_cannot_buy_a_card_for_themselves()
    {
        var create = () => new GiftCard(WellKnownUsers.Explorer, WellKnownUsers.Explorer, 50, null, PurchaseTime);

        create.Should().Throw<DomainException>();
    }

    [Fact]
    public void A_card_is_valid_until_three_years_after_the_purchase()
    {
        var giftCard = CreateGiftCard();

        var status = giftCard.StatusAt(PurchaseTime.AddYears(3).AddTicks(-1));

        status.Should().Be(GiftCardStatus.Valid);
    }

    [Fact]
    public void A_card_is_expired_from_three_years_after_the_purchase()
    {
        var giftCard = CreateGiftCard();

        var status = giftCard.StatusAt(PurchaseTime.AddYears(3));

        status.Should().Be(GiftCardStatus.Expired);
    }

    [Fact]
    public void A_redeemed_card_cannot_be_redeemed_again()
    {
        var giftCard = CreateGiftCard();
        var redeemedAt = PurchaseTime.AddDays(1);
        giftCard.Redeem(WellKnownUsers.SecondExplorer, redeemedAt);

        var redeem = () => giftCard.Redeem(WellKnownUsers.SecondExplorer, redeemedAt.AddDays(1));

        redeem.Should().Throw<DomainException>();
        giftCard.RedeemedAt.Should().Be(redeemedAt);
    }

    [Fact]
    public void A_card_cannot_be_redeemed_by_someone_other_than_its_recipient()
    {
        var giftCard = CreateGiftCard();

        var redeem = () => giftCard.Redeem(WellKnownUsers.ThirdExplorer, PurchaseTime.AddDays(1));

        redeem.Should().Throw<DomainException>();
        giftCard.RedeemedAt.Should().BeNull();
    }

    [Fact]
    public void A_redeemed_card_stays_redeemed_after_it_would_have_expired()
    {
        var giftCard = CreateGiftCard();
        giftCard.Redeem(WellKnownUsers.SecondExplorer, PurchaseTime.AddDays(1));

        var status = giftCard.StatusAt(PurchaseTime.AddYears(3));

        status.Should().Be(GiftCardStatus.Redeemed);
    }

    private static GiftCard CreateGiftCard()
    {
        return new GiftCard(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer, 50, null, PurchaseTime);
    }
}
