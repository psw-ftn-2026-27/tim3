using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.Wallets;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.AdminWallet;

public class AdminWalletQueryTests : BaseIntegrationTest
{
    public AdminWalletQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_can_read_a_page_of_the_selected_wallet_history()
    {
        var expected = WalletSeed.ExplorerWalletWithReversal;
        var expectedTransaction = expected.Transactions.OrderByDescending(item => item.OccurredAt).ElementAt(1);
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var wallet = await client.GetFromJsonAsync<WalletDto>(
            $"/api/payment/wallet/{expected.UserId}?page=2&pageSize=1", JsonOptions);

        wallet!.Balance.Should().Be(expected.Balance);
        wallet.Transactions.TotalCount.Should().Be(expected.Transactions.Count);
        wallet.Transactions.Items.Should().ContainSingle().Which.Id.Should().Be(expectedTransaction.Id);
    }

    [Fact]
    public async Task Explorer_cannot_read_another_users_wallet()
    {
        var expected = WalletSeed.ExplorerWalletWithReversal;
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync($"/api/payment/wallet/{expected.UserId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
