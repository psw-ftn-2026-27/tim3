using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Payment.Application;
using Payment.Application.ShoppingCart;
using Payment.Application.Wallets;
using Payment.Domain.ShoppingCarts;
using Payment.Domain.Wallets;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;
using Cart = Payment.Domain.ShoppingCarts.ShoppingCart;

namespace Payment.Tests.Integration.ShoppingCart;

public sealed class ShoppingCartCheckoutTests : BaseIntegrationTest
{
    private const string CheckoutUrl = "/api/payment/cart/checkout";

    public ShoppingCartCheckoutTests(PaymentApiFactory factory) : base(factory)
    {
        Factory.Reseed<DbContext>(CheckoutTourSeed.All);
    }

    [Fact]
    public async Task Buying_paid_and_free_tours_persists_every_effect()
    {
        var cart = ShoppingCartSeed.FundedCheckoutCart;
        var wallet = WalletSeed.ExplorerWalletWithDeposit;
        using var client = Factory.CreateClientFor(cart.UserId, "explorer");

        var response = await client.PostAsync(CheckoutUrl, null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var savedCart = await context.ShoppingCarts.SingleAsync(row => row.Id == cart.Id);
        savedCart.Items.Should().BeEmpty();
        var savedWallet = await context.Wallets.Include(row => row.Transactions).SingleAsync(row => row.Id == wallet.Id);
        savedWallet.Balance.Should().Be(wallet.Balance - (int)cart.TotalInEc);
        savedWallet.Transactions.Should().Contain(wallet.Transactions.Single());
        var purchases = savedWallet.Transactions.Where(row => row.Kind == WalletTransactionKind.Purchase).ToList();
        purchases.Select(row => new { row.TourId, row.Amount }).Should().BeEquivalentTo(
            cart.Items.Select(item => new { TourId = (Guid?)item.TourId, Amount = -item.PriceInEc }));
        var tokens = await context.TourPurchaseTokens.Where(row => row.BuyerId == cart.UserId).ToListAsync();
        tokens.Select(row => row.TourId).Should().BeEquivalentTo(cart.Items.Select(item => item.TourId));
        tokens.Should().OnlyContain(token =>
            purchases.Any(row => row.TourId == token.TourId && row.OccurredAt == token.PurchasedAt));

    }

    [Fact]
    public async Task A_free_purchase_without_a_wallet_records_zero_and_grants_ownership()
    {
        var cart = ShoppingCartSeed.FreeCheckoutCart;
        var product = ProductSeed.FreeWalk;
        using var client = Factory.CreateClientFor(cart.UserId, "explorer");

        var response = await client.PostAsync(CheckoutUrl, null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var wallet = await context.Wallets.Include(row => row.Transactions).SingleAsync(row => row.UserId == cart.UserId);
        wallet.Balance.Should().Be(0);
        var purchase = wallet.Transactions.Should().ContainSingle().Which;
        purchase.Kind.Should().Be(WalletTransactionKind.Purchase);
        purchase.Amount.Should().Be(0);
        purchase.TourId.Should().Be(product.TourId);
        var token = await context.TourPurchaseTokens.SingleAsync(row => row.BuyerId == cart.UserId);
        token.TourId.Should().Be(product.TourId);
        (await context.ShoppingCarts.SingleAsync(row => row.Id == cart.Id)).Items.Should().BeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Unfunded_purchase_leaves_the_database_intact(bool hasWallet)
    {
        var cart = hasWallet ? ShoppingCartSeed.ExplorerCart : ShoppingCartSeed.OtherExplorerCart;
        var wallet = hasWallet ? WalletSeed.EmptyExplorerWallet : null;
        using var client = Factory.CreateClientFor(cart.UserId, "explorer");

        var response = await client.PostAsync(CheckoutUrl, null);

        await AssertRejectedAsync(response, "dovoljno EC");
        await AssertCheckoutUnchangedAsync(cart, wallet);
    }

    [Fact]
    public async Task A_tour_the_buyer_already_owns_cannot_be_purchased_again()
    {
        var cart = ShoppingCartSeed.AlreadyPurchasedCart;
        using var client = Factory.CreateClientFor(cart.UserId, "explorer");

        var response = await client.PostAsync(CheckoutUrl, null);

        await AssertRejectedAsync(response, "poseduješ");
        await AssertCheckoutUnchangedAsync(cart, WalletSeed.ExplorerWalletWithReversal);
    }

    [Fact]
    public async Task One_archived_tour_blocks_the_entire_purchase()
    {
        var cart = ShoppingCartSeed.ArchivedCheckoutCart;
        using var client = Factory.CreateClientFor(cart.UserId, "explorer");

        var response = await client.PostAsync(CheckoutUrl, null);

        await AssertRejectedAsync(response, ProductSeed.ArchivedWalk.Name);
        await AssertCheckoutUnchangedAsync(cart, WalletSeed.DemoExplorerWalletWithDeposit);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("administrator", HttpStatusCode.Forbidden)]
    public async Task Only_authenticated_explorers_can_checkout(string? role, HttpStatusCode expected)
    {
        using var client = role is null
            ? Factory.CreateClient()
            : Factory.CreateClientFor(WellKnownUsers.Administrator, role);

        var response = await client.PostAsync(CheckoutUrl, null);

        response.StatusCode.Should().Be(expected);
    }

    [Fact]
    public async Task An_availability_failure_blocks_the_entire_purchase()
    {
        var cart = ShoppingCartSeed.FundedCheckoutCart;
        var availability = new TestTourAvailability(new HashSet<Guid>(), new InvalidOperationException("Availability unavailable."));

        var checkout = () => CheckoutAsync(cart.UserId, availability);

        await checkout.Should().ThrowAsync<InvalidOperationException>();
        await AssertCheckoutUnchangedAsync(cart, WalletSeed.ExplorerWalletWithDeposit);
    }

    [Fact]
    public async Task A_database_failure_at_commit_rolls_back_every_checkout_effect()
    {
        var cart = ShoppingCartSeed.FundedCheckoutCart;
        var availability = new TestTourAvailability(new HashSet<Guid> { ProductSeed.CityWalk.TourId, ProductSeed.FreeWalk.TourId });
        using var context = Factory.CreateContext<PaymentDbContext>();
        await context.Database.ExecuteSqlRawAsync("""
            CREATE FUNCTION payment.reject_checkout_test() RETURNS trigger LANGUAGE plpgsql AS $$
            BEGIN
                RAISE EXCEPTION 'Checkout test commit failure' USING ERRCODE = '23514';
            END;
            $$;
            CREATE CONSTRAINT TRIGGER reject_checkout_test
            AFTER INSERT ON payment."TourPurchaseTokens"
            DEFERRABLE INITIALLY DEFERRED
            FOR EACH ROW EXECUTE FUNCTION payment.reject_checkout_test();
            """);

        try
        {
            var failure = await Record.ExceptionAsync(() => CheckoutAsync(cart.UserId, availability));

            var postgresFailure = failure!.GetBaseException().Should().BeOfType<PostgresException>().Which;
            postgresFailure.SqlState.Should().Be(PostgresErrorCodes.CheckViolation);
            await AssertCheckoutUnchangedAsync(cart, WalletSeed.ExplorerWalletWithDeposit);
        }
        finally
        {
            await context.Database.ExecuteSqlRawAsync("""
                DROP TRIGGER reject_checkout_test ON payment."TourPurchaseTokens";
                DROP FUNCTION payment.reject_checkout_test();
                """);
        }
    }

    private async Task CheckoutAsync(Guid userId, ITourAvailability availability)
    {
        using var scope = Factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var checkout = new ShoppingCartCheckoutService(
            services.GetRequiredService<IShoppingCartRepository>(),
            services.GetRequiredService<IWalletRepository>(),
            services.GetRequiredService<ITourPurchaseTokenRepository>(),
            availability,
            services.GetRequiredService<CartPurchasing>(),
            services.GetRequiredService<IUnitOfWork>());
        await checkout.CheckoutAsync(userId);
    }

    private static async Task AssertRejectedAsync(HttpResponseMessage response, string expectedReason)
    {
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);
        problem!.Title.Should().Contain(expectedReason);
    }

    private async Task AssertCheckoutUnchangedAsync(Cart cart, Wallet? wallet)
    {
        using var context = Factory.CreateContext<PaymentDbContext>();
        var savedCart = await context.ShoppingCarts.SingleAsync(row => row.Id == cart.Id);
        savedCart.Items.Should().BeEquivalentTo(cart.Items);
        var savedWallet = await context.Wallets.Include(row => row.Transactions)
            .SingleOrDefaultAsync(row => row.UserId == cart.UserId);
        if (wallet is null)
        {
            savedWallet.Should().BeNull();
        }
        else
        {
            savedWallet.Should().NotBeNull();
            savedWallet!.Balance.Should().Be(wallet.Balance);
            savedWallet.Transactions.Should().BeEquivalentTo(wallet.Transactions);
        }

        var tokens = await context.TourPurchaseTokens.Where(row => row.BuyerId == cart.UserId).ToListAsync();
        tokens.Should().BeEquivalentTo(TourPurchaseTokenSeed.All.Where(row => row.BuyerId == cart.UserId));
    }

    private sealed class TestTourAvailability : ITourAvailability
    {
        private readonly IReadOnlySet<Guid> _availableTours;
        private readonly Exception? _failure;

        public TestTourAvailability(IReadOnlySet<Guid> availableTours, Exception? failure = null)
        {
            _availableTours = availableTours;
            _failure = failure;
        }

        public Task<bool> IsAvailableAsync(Guid tourId)
        {
            return _failure is null
                ? Task.FromResult(_availableTours.Contains(tourId))
                : Task.FromException<bool>(_failure);
        }
    }
}
