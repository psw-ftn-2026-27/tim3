using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.OwnWallet;

public sealed class OwnWalletCommandTests : BaseIntegrationTest
{
    public OwnWalletCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_can_create_their_missing_wallet()
    {
        var userId = WellKnownUsers.ThirdExplorer;
        using var client = Factory.CreateClientFor(userId, "explorer");

        var response = await client.PostAsync("/api/payment/wallet/me", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var wallet = await context.Wallets.Include(wallet => wallet.Transactions)
            .SingleAsync(wallet => wallet.UserId == userId);
        wallet.Balance.Should().Be(0);
        wallet.Transactions.Should().BeEmpty();
    }

    [Fact]
    public async Task Retrying_wallet_creation_preserves_existing_funds_and_history()
    {
        var existing = WalletSeed.ExplorerWalletWithReversal;
        using var client = Factory.CreateClientFor(existing.UserId, "explorer");

        var response = await client.PostAsync("/api/payment/wallet/me", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var wallet = await context.Wallets.Include(wallet => wallet.Transactions)
            .SingleAsync(wallet => wallet.UserId == existing.UserId);
        wallet.Id.Should().Be(existing.Id);
        wallet.Balance.Should().Be(existing.Balance);
        wallet.Transactions.Should().BeEquivalentTo(existing.Transactions);
    }

    [Fact]
    public async Task Simultaneous_attempts_create_only_one_wallet_for_the_same_explorer()
    {
        var userId = WellKnownUsers.ThirdExplorer;
        using var client = Factory.CreateClientFor(userId, "explorer");

        var responses = await Task.WhenAll(
            client.PostAsync("/api/payment/wallet/me", null),
            client.PostAsync("/api/payment/wallet/me", null));

        responses.Should().OnlyContain(response => response.StatusCode == HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var wallets = await context.Wallets.CountAsync(wallet => wallet.UserId == userId);
        wallets.Should().Be(1);
    }

    [Fact]
    public async Task Anonymous_visitor_cannot_create_a_wallet()
    {
        using var client = Factory.CreateClient();

        var response = await client.PostAsync("/api/payment/wallet/me", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
