using FluentAssertions;
using Payment.Domain.Coupons;
using Payment.Domain.GiftCards;
using Payment.Domain.Products;
using Payment.Domain.ShoppingCarts;
using Xunit;

namespace Payment.Tests.Unit.ShoppingCarts;

public class CartPricingTests
{
    private static readonly DateTime Now = new(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid Buyer = Guid.NewGuid();
    private static readonly Guid Author = Guid.NewGuid();

    [Theory]
    [InlineData(10, 10, 0, 15)]
    [InlineData(30, 25, 5, 0)]
    public void A_gift_card_reduces_the_total_by_its_amount_but_not_below_zero(
        int amount, int applied, int unused, long fromWallet)
    {
        var cart = new ShoppingCart(Buyer);
        cart.AddItem(CreateProduct(25));
        var giftCard = CreateGiftCard(amount);
        EnterGiftCard(cart, giftCard);

        var price = CreatePricing().Price(cart, [], new HashSet<Guid>(), giftCard, Now);

        price.GiftCard.Should().BeEquivalentTo(new
        {
            IsApplied = true, AppliedInEc = applied, UnusedInEc = unused
        });
        price.TotalAfterCouponsInEc.Should().Be(25);
        price.TotalFromWalletInEc.Should().Be(fromWallet);
    }

    [Fact]
    public void A_gift_card_is_applied_to_the_total_left_after_the_coupons()
    {
        var cart = new ShoppingCart(Buyer);
        var product = CreateProduct(40);
        cart.AddItem(product);
        var coupon = CreateCoupon(product.Id, 25);
        EnterCoupon(cart, coupon);
        var giftCard = CreateGiftCard(50);
        EnterGiftCard(cart, giftCard);

        var price = CreatePricing().Price(cart, [coupon], new HashSet<Guid>(), giftCard, Now);

        price.TotalAfterCouponsInEc.Should().Be(30);
        price.GiftCard.Should().BeEquivalentTo(new { AppliedInEc = 30, UnusedInEc = 20 });
        price.TotalFromWalletInEc.Should().Be(0);
    }

    [Fact]
    public void A_gift_card_is_not_applied_when_the_total_after_the_coupons_is_zero()
    {
        var cart = new ShoppingCart(Buyer);
        var product = CreateProduct(20);
        cart.AddItem(product);
        var coupon = CreateCoupon(product.Id, 100);
        EnterCoupon(cart, coupon);
        var giftCard = CreateGiftCard(10);
        EnterGiftCard(cart, giftCard);

        var price = CreatePricing().Price(cart, [coupon], new HashSet<Guid>(), giftCard, Now);

        price.GiftCard.Should().BeEquivalentTo(new
        {
            IsApplied = false, AppliedInEc = 0, Reason = GiftCardNotAppliedReason.TotalIsZero
        });
        price.TotalFromWalletInEc.Should().Be(0);
    }

    [Theory]
    [InlineData(true, GiftCardNotAppliedReason.Expired)]
    [InlineData(false, GiftCardNotAppliedReason.Redeemed)]
    public void An_expired_or_redeemed_gift_card_is_shown_as_not_applied_with_the_reason(
        bool expired, GiftCardNotAppliedReason reason)
    {
        var cart = new ShoppingCart(Buyer);
        cart.AddItem(CreateProduct(25));
        var giftCard = CreateGiftCard(10);
        EnterGiftCard(cart, giftCard);
        if (!expired)
        {
            giftCard.Redeem(Buyer, Now);
        }
        var moment = expired ? giftCard.ExpiresAt : Now;

        var price = CreatePricing().Price(cart, [], new HashSet<Guid>(), giftCard, moment);

        price.GiftCard.Should().BeEquivalentTo(new { IsApplied = false, AppliedInEc = 0, Reason = reason });
        price.TotalFromWalletInEc.Should().Be(25);
    }

    [Fact]
    public void A_gift_card_received_by_another_buyer_is_shown_as_not_applied()
    {
        var cart = new ShoppingCart(Buyer);
        cart.AddItem(CreateProduct(25));
        var othersGiftCard = new GiftCard(Guid.NewGuid(), Guid.NewGuid(), 10, null, Now.AddDays(-1));

        var price = CreatePricing().Price(cart, [], new HashSet<Guid>(), othersGiftCard, Now);

        price.GiftCard.Should().BeEquivalentTo(new
        {
            IsApplied = false, AppliedInEc = 0, Reason = GiftCardNotAppliedReason.NotRecipient
        });
        price.TotalFromWalletInEc.Should().Be(25);
    }

    [Fact]
    public void The_gift_card_is_spread_over_the_tours_in_the_order_they_were_added_up_to_their_price_after_coupons()
    {
        var cart = new ShoppingCart(Buyer);
        var free = CreateProduct(0);
        var coveredByCoupon = CreateProduct(20);
        var first = CreateProduct(15);
        var second = CreateProduct(30);
        cart.AddItem(free);
        cart.AddItem(coveredByCoupon);
        cart.AddItem(first);
        cart.AddItem(second);
        var coupon = CreateCoupon(coveredByCoupon.Id, 100);
        EnterCoupon(cart, coupon);
        var giftCard = CreateGiftCard(25);
        EnterGiftCard(cart, giftCard);

        var price = CreatePricing().Price(cart, [coupon], new HashSet<Guid>(), giftCard, Now);

        new[] { free, coveredByCoupon, first, second }.Select(product => price.GiftCardOn(product.TourId))
            .Should().Equal(0, 0, 15, 10);
    }

    private static CartPricing CreatePricing()
    {
        return new CartPricing(new CouponAllocation());
    }

    private static void EnterGiftCard(ShoppingCart cart, GiftCard giftCard)
    {
        new CartPurchasing(CreatePricing()).EnterGiftCard(cart, giftCard, giftCard.PurchasedAt);
    }

    private static void EnterCoupon(ShoppingCart cart, Coupon coupon)
    {
        new CartPurchasing(CreatePricing()).EnterCoupon(cart, coupon, new HashSet<Guid>(), DateOnly.FromDateTime(Now));
    }

    private static GiftCard CreateGiftCard(int amount)
    {
        return new GiftCard(Guid.NewGuid(), Buyer, amount, null, Now.AddDays(-1));
    }

    private static Product CreateProduct(int priceInEc)
    {
        return new Product(Guid.NewGuid(), "City walk", Author, priceInEc);
    }

    private static Coupon CreateCoupon(Guid productId, int discountPercent)
    {
        return new Coupon(
            Author, new CouponRequest(productId, discountPercent, DateOnly.FromDateTime(Now).AddDays(10)),
            CouponCode.Generate(), Now.AddDays(-1));
    }
}
