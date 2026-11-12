using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.Wallets;
using Payment.Domain.Wallets;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.OwnWallet;

public class OwnWalletQueryTests : BaseIntegrationTest
{
    public OwnWalletQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_can_see_their_empty_wallet()
    {
        var expected = WalletSeed.EmptyExplorerWallet;
        var client = Factory.CreateClientFor(expected.UserId, "explorer");

        var wallet = await client.GetFromJsonAsync<WalletDto>("/api/payment/wallet/me", JsonOptions);

        wallet!.Balance.Should().Be(expected.Balance);
        wallet.Transactions.Items.Should().BeEmpty();
        wallet.Transactions.TotalCount.Should().Be(expected.Transactions.Count);
    }

    [Fact]
    public async Task Explorer_can_see_a_reversal_in_their_wallet_history()
    {
        var expected = WalletSeed.ExplorerWalletWithReversal;
        var reversal = expected.Transactions.Single(transaction => transaction.Kind == WalletTransactionKind.Reversal);
        var client = Factory.CreateClientFor(expected.UserId, "explorer");

        var wallet = await client.GetFromJsonAsync<WalletDto>("/api/payment/wallet/me", JsonOptions);

        wallet!.Balance.Should().Be(expected.Balance);
        wallet.Transactions.Items.Should().ContainSingle(transaction => transaction.Id == reversal.Id)
            .Which.Should().Be(new WalletTransactionDto(reversal.Id, reversal.Amount, reversal.OccurredAt, WalletTransactionKind.Reversal));
    }

    [Fact]
    public async Task Explorer_without_a_wallet_cannot_see_another_users_wallet()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");

        var response = await client.GetAsync("/api/payment/wallet/me");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Anonymous_visitor_cannot_see_a_wallet()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/payment/wallet/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
