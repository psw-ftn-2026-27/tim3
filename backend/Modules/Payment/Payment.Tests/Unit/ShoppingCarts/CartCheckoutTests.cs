using FluentAssertions;
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

        var tokens = new CartPurchasing().Checkout(cart, wallet, available, new HashSet<Guid>(), purchasedAt);

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

        var checkout = () => new CartPurchasing().Checkout(cart, wallet, availability, purchasedTours, now);

        checkout.Should().Throw<DomainException>();
        cart.Items.Should().ContainSingle().Which.TourId.Should().Be(product.TourId);
        wallet.Balance.Should().Be(balance);
        wallet.Transactions.Should().ContainSingle().Which.Kind.Should().Be(WalletTransactionKind.Deposit);
    }
}
