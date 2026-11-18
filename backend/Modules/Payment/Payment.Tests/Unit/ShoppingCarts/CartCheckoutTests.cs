using FluentAssertions;
using Payment.Domain.Coupons;
using Payment.Domain.Products;
using Payment.Domain.ShoppingCarts;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;
using Xunit;

namespace Payment.Tests.Unit.ShoppingCarts;

public sealed class CartCheckoutTests
{
    [Fact]
    public void Checkout_charges_each_tour_and_issues_one_token_per_item()
    {
        var userId = Guid.NewGuid();
        var cart = new ShoppingCart(userId);
        var paid = new Product(Guid.NewGuid(), "City walk", Guid.NewGuid(), 25);
        var free = new Product(Guid.NewGuid(), "Free walk", paid.AuthorId, 0);
        cart.AddItem(paid);
        cart.AddItem(free);
        var wallet = new Wallet(userId);
        var purchasedAt = DateTime.UtcNow;
        wallet.Deposit(40, Guid.NewGuid(), purchasedAt.AddHours(-1));
        HashSet<Guid> available = [paid.TourId, free.TourId];

        var tokens = CreatePurchasing().Checkout(
            cart, wallet, [], NoHistory(), available, 25, purchasedAt);

        cart.Items.Should().BeEmpty();
        wallet.Balance.Should().Be(15);
        wallet.Transactions.Where(row => row.Kind == WalletTransactionKind.Purchase)
            .Select(row => new { row.TourId, row.Amount, row.OccurredAt })
            .Should().BeEquivalentTo(new[]
            {
                new { TourId = (Guid?)paid.TourId, Amount = -25, OccurredAt = purchasedAt },
                new { TourId = (Guid?)free.TourId, Amount = 0, OccurredAt = purchasedAt }
            });
        tokens.Select(token => token.TourId).Should().BeEquivalentTo(available);
        tokens.Should().OnlyContain(token => token.BuyerId == userId && token.PurchasedAt == purchasedAt);
    }

    [Theory]
    [InlineData(24, true, false)]
    [InlineData(40, false, false)]
    [InlineData(40, true, true)]
    public void Rejected_checkout_leaves_the_cart_and_wallet_intact(int balance, bool available, bool purchased)
    {
        var userId = Guid.NewGuid();
        var cart = new ShoppingCart(userId);
        var product = new Product(Guid.NewGuid(), "City walk", Guid.NewGuid(), 25);
        cart.AddItem(product);
        var wallet = new Wallet(userId);
        var now = DateTime.UtcNow;
        wallet.Deposit(balance, Guid.NewGuid(), now.AddHours(-1));
        HashSet<Guid> availability = available ? [product.TourId] : [];
        HashSet<Guid> purchasedTours = purchased ? [product.TourId] : [];

        var checkout = () => CreatePurchasing().Checkout(
            cart, wallet, [], new BuyerHistory(purchasedTours, new HashSet<Guid>()), availability, 25, now);

        checkout.Should().Throw<DomainException>();
        cart.Items.Should().ContainSingle().Which.TourId.Should().Be(product.TourId);
        wallet.Balance.Should().Be(balance);
        wallet.Transactions.Should().ContainSingle().Which.Kind.Should().Be(WalletTransactionKind.Deposit);
    }

    [Fact]
    public void A_purchase_is_refused_when_the_total_differs_from_the_one_the_buyer_saw()
    {
        var userId = Guid.NewGuid();
        var cart = new ShoppingCart(userId);
        var product = new Product(Guid.NewGuid(), "City walk", Guid.NewGuid(), 25);
        cart.AddItem(product);
        var now = DateTime.UtcNow;
        var coupon = CreateCoupon(product.AuthorId, 20, now);
        CreatePurchasing().EnterCoupon(cart, coupon, new HashSet<Guid>(), Today(now));
        var wallet = new Wallet(userId);
        wallet.Deposit(40, Guid.NewGuid(), now.AddHours(-1));

        var checkout = () => CreatePurchasing().Checkout(
            cart, wallet, [coupon], NoHistory(), new HashSet<Guid> { product.TourId }, 25, now);

        checkout.Should().Throw<DomainException>();
        cart.Items.Should().ContainSingle();
        cart.CouponIds.Should().Equal(coupon.Id);
        wallet.Balance.Should().Be(40);
    }

    [Fact]
    public void A_purchase_with_a_coupon_charges_the_discounted_total_records_the_coupon_and_clears_all_coupons()
    {
        var userId = Guid.NewGuid();
        var cart = new ShoppingCart(userId);
        var discounted = new Product(Guid.NewGuid(), "City walk", Guid.NewGuid(), 25);
        var fullPrice = new Product(Guid.NewGuid(), "Museum walk", Guid.NewGuid(), 15);
        cart.AddItem(discounted);
        cart.AddItem(fullPrice);
        var now = DateTime.UtcNow;
        var applied = CreateCoupon(discounted.AuthorId, 20, now);
        var notApplied = CreateCoupon(discounted.AuthorId, 10, now);
        CreatePurchasing().EnterCoupon(cart, applied, new HashSet<Guid>(), Today(now));
        CreatePurchasing().EnterCoupon(cart, notApplied, new HashSet<Guid>(), Today(now));
        var wallet = new Wallet(userId);
        wallet.Deposit(40, Guid.NewGuid(), now.AddHours(-1));
        HashSet<Guid> available = [discounted.TourId, fullPrice.TourId];

        var tokens = CreatePurchasing().Checkout(
            cart, wallet, [applied, notApplied], NoHistory(), available, 35, now);

        wallet.Balance.Should().Be(5);
        wallet.Transactions.Where(row => row.Kind == WalletTransactionKind.Purchase)
            .Select(row => new { row.TourId, row.Amount, row.DiscountInEc })
            .Should().BeEquivalentTo(new[]
            {
                new { TourId = (Guid?)discounted.TourId, Amount = -20, DiscountInEc = 5 },
                new { TourId = (Guid?)fullPrice.TourId, Amount = -15, DiscountInEc = 0 }
            });
        tokens.Select(token => new { token.TourId, token.CouponId, token.DiscountInEc })
            .Should().BeEquivalentTo(new[]
            {
                new { discounted.TourId, CouponId = (Guid?)applied.Id, DiscountInEc = 5 },
                new { fullPrice.TourId, CouponId = (Guid?)null, DiscountInEc = 0 }
            });
        cart.Items.Should().BeEmpty();
        cart.CouponIds.Should().BeEmpty();
    }

    private static CartPurchasing CreatePurchasing()
    {
        return new CartPurchasing(new CouponAllocation());
    }

    private static BuyerHistory NoHistory()
    {
        return new BuyerHistory(new HashSet<Guid>(), new HashSet<Guid>());
    }

    private static DateOnly Today(DateTime now)
    {
        return DateOnly.FromDateTime(now);
    }

    private static Coupon CreateCoupon(Guid authorId, int discountPercent, DateTime now)
    {
        return new Coupon(
            authorId, new CouponRequest(null, discountPercent, Today(now).AddDays(10)), CouponCode.Generate(), now);
    }
}
