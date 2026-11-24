using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.AdminWallet;
using Payment.Application.Wallets;
using Payment.Tests.Integration.Seeds;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.AdminWallet;

public class AdminWalletQueryTests : BaseIntegrationTest
{
    // Identity seeds admin1 in the Development test host.
    private static readonly Guid DemoAdministratorId = Guid.Parse("10000000-0000-0000-0000-000000000001");

    public AdminWalletQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_sees_all_accounts_with_their_wallet_balances()
    {
        var funded = WalletSeed.DemoExplorerWalletWithDeposit;
        using var client = Factory.CreateClientFor(DemoAdministratorId, "administrator");

        var page = await client.GetFromJsonAsync<PageResult<UserWalletSummaryDto>>(
            "/api/payment/wallet/users", JsonOptions);

        page!.Items.Should().Contain(new UserWalletSummaryDto(funded.UserId, "explorer1", funded.Balance));
        page.Items.Should().Contain(new UserWalletSummaryDto(DemoAdministratorId, "admin1", 0));
        page.TotalCount.Should().Be(page.Items.Count);
    }

    [Fact]
    public async Task Administrator_can_search_by_display_name_and_view_the_next_page()
    {
        using var client = Factory.CreateClientFor(DemoAdministratorId, "administrator");

        var page = await client.GetFromJsonAsync<PageResult<UserWalletSummaryDto>>(
            "/api/payment/wallet/users?text=%20eXpLoReR%20&page=2&pageSize=1", JsonOptions);

        page!.Items.Should().ContainSingle().Which.DisplayName.Should().Be("explorer2");
        page.TotalCount.Should().BeGreaterThan(page.Items.Count);
    }

    [Fact]
    public async Task Searching_for_an_email_domain_does_not_match_an_account()
    {
        using var client = Factory.CreateClientFor(DemoAdministratorId, "administrator");

        var page = await client.GetFromJsonAsync<PageResult<UserWalletSummaryDto>>(
            "/api/payment/wallet/users?text=explorer.dev", JsonOptions);

        page!.Items.Should().BeEmpty();
        page.TotalCount.Should().Be(page.Items.Count);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("explorer", HttpStatusCode.Forbidden)]
    public async Task Only_administrators_can_browse_users_wallets(string? role, HttpStatusCode expected)
    {
        using var client = role is null
            ? Factory.CreateClient()
            : Factory.CreateClientFor(WellKnownUsers.Explorer, role);

        var response = await client.GetAsync("/api/payment/wallet/users");

        response.StatusCode.Should().Be(expected);
    }

    [Fact]
    public async Task Older_account_without_a_wallet_has_zero_balance_and_empty_history()
    {
        using var client = Factory.CreateClientFor(DemoAdministratorId, "administrator");

        var wallet = await client.GetFromJsonAsync<WalletDto>(
            $"/api/payment/wallet/{DemoAdministratorId}", JsonOptions);

        wallet!.Balance.Should().Be(0);
        wallet.Transactions.Items.Should().BeEmpty();
        wallet.Transactions.TotalCount.Should().Be(wallet.Transactions.Items.Count);
    }

    [Fact]
    public async Task Administrator_cannot_open_a_wallet_for_an_unknown_user()
    {
        using var client = Factory.CreateClientFor(DemoAdministratorId, "administrator");
        var unknownUserId = Guid.NewGuid();

        var response = await client.GetAsync($"/api/payment/wallet/{unknownUserId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

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
