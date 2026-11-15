using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Payment.Application.AdminWallet;
using Payment.Domain.Wallets;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.AdminWallet;

public class AdminWalletCommandTests : BaseIntegrationTest
{
    public AdminWalletCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_adds_a_deposit_to_the_recipients_wallet()
    {
        var wallet = WalletSeed.DemoExplorerWalletWithDeposit;
        var previousDeposit = wallet.Transactions.Single();
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var deposit = new DepositEcDto(60);
        var startedAt = DateTime.UtcNow;

        var response = await client.PostAsJsonAsync($"/api/payment/wallet/{wallet.UserId}/deposits", deposit);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var stored = await context.Wallets.Include(item => item.Transactions).SingleAsync(item => item.Id == wallet.Id);
        stored.Balance.Should().Be(wallet.Balance + deposit.Amount);
        stored.Transactions.Should().HaveCount(wallet.Transactions.Count + 1).And.Contain(previousDeposit);
        var newDeposit = stored.Transactions.Single(item => item.Id != previousDeposit.Id);
        newDeposit.Amount.Should().Be(deposit.Amount);
        newDeposit.Kind.Should().Be(WalletTransactionKind.Deposit);
        newDeposit.AdministratorId.Should().Be(WellKnownUsers.Administrator);
        newDeposit.OccurredAt.Should().BeOnOrAfter(startedAt.AddMilliseconds(-1)).And.BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public async Task Rejected_amount_leaves_the_recipients_wallet_unchanged()
    {
        var wallet = WalletSeed.DemoExplorerWalletWithDeposit;
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsJsonAsync($"/api/payment/wallet/{wallet.UserId}/deposits", new DepositEcDto(0));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var stored = await context.Wallets.Include(item => item.Transactions).SingleAsync(item => item.Id == wallet.Id);
        stored.Balance.Should().Be(wallet.Balance);
        stored.Transactions.Should().BeEquivalentTo(wallet.Transactions);
    }

    [Fact]
    public async Task First_deposit_creates_the_recipients_wallet_with_the_deposit()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var demoExplorerWithoutWalletId = Guid.Parse("20000000-0000-0000-0000-000000000002");
        var deposit = new DepositEcDto(100);

        var response = await client.PostAsJsonAsync($"/api/payment/wallet/{demoExplorerWithoutWalletId}/deposits", deposit);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var stored = await context.Wallets.Include(wallet => wallet.Transactions)
            .SingleAsync(wallet => wallet.UserId == demoExplorerWithoutWalletId);
        stored.Balance.Should().Be(deposit.Amount);
        var transaction = stored.Transactions.Should().ContainSingle().Which;
        transaction.Amount.Should().Be(deposit.Amount);
        transaction.Kind.Should().Be(WalletTransactionKind.Deposit);
        transaction.AdministratorId.Should().Be(WellKnownUsers.Administrator);
    }

    [Fact]
    public async Task Depositing_for_an_unknown_user_writes_no_wallet_or_transaction()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var userId = Guid.NewGuid();
        int previousTransactionCount;
        using (var initialContext = Factory.CreateContext<PaymentDbContext>())
        {
            previousTransactionCount = await initialContext.Set<WalletTransaction>().CountAsync();
        }

        var response = await client.PostAsJsonAsync($"/api/payment/wallet/{userId}/deposits", new DepositEcDto(60));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var context = Factory.CreateContext<PaymentDbContext>();
        (await context.Wallets.AnyAsync(wallet => wallet.UserId == userId)).Should().BeFalse();
        (await context.Set<WalletTransaction>().CountAsync()).Should().Be(previousTransactionCount);
    }

    [Fact]
    public async Task Anonymous_visitor_cannot_deposit_ec()
    {
        var client = Factory.CreateClient();
        var userId = WalletSeed.ExplorerWalletWithDeposit.UserId;

        var response = await client.PostAsJsonAsync($"/api/payment/wallet/{userId}/deposits", new DepositEcDto(60));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_deposit_ec()
    {
        var userId = WalletSeed.ExplorerWalletWithDeposit.UserId;
        var client = Factory.CreateClientFor(userId, "explorer");

        var response = await client.PostAsJsonAsync($"/api/payment/wallet/{userId}/deposits", new DepositEcDto(60));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_reverses_a_deposit_without_removing_the_original()
    {
        var wallet = WalletSeed.ExplorerWalletWithDeposit;
        var deposit = wallet.Transactions.Single();
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var startedAt = DateTime.UtcNow;

        var response = await client.PostAsync($"/api/payment/wallet/{wallet.UserId}/deposits/{deposit.Id}/reverse", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var stored = await context.Wallets.Include(item => item.Transactions).SingleAsync(item => item.Id == wallet.Id);
        stored.Balance.Should().Be(wallet.Balance - deposit.Amount);
        stored.Transactions.Should().HaveCount(wallet.Transactions.Count + 1).And.Contain(deposit);
        var reversal = stored.Transactions.Single(item => item.Id != deposit.Id);
        reversal.Kind.Should().Be(WalletTransactionKind.Reversal);
        reversal.OriginalDepositId.Should().Be(deposit.Id);
        reversal.Amount.Should().Be(-deposit.Amount);
        reversal.AdministratorId.Should().Be(WellKnownUsers.Administrator);
        reversal.OccurredAt.Should().BeOnOrAfter(startedAt.AddMilliseconds(-1)).And.BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public async Task An_expired_deposit_remains_in_the_wallet_without_a_reversal()
    {
        var wallet = WalletSeed.ExplorerWalletWithReversal;
        var deposit = wallet.Transactions.Single(item => item.Kind == WalletTransactionKind.Deposit && item.Amount == 20);
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/payment/wallet/{wallet.UserId}/deposits/{deposit.Id}/reverse", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var stored = await context.Wallets.Include(item => item.Transactions).SingleAsync(item => item.Id == wallet.Id);
        stored.Balance.Should().Be(wallet.Balance);
        stored.Transactions.Should().BeEquivalentTo(wallet.Transactions);
    }

    [Fact]
    public async Task A_reversal_requires_an_existing_wallet()
    {
        var wallet = WalletSeed.ExplorerWalletWithDeposit;
        var deposit = wallet.Transactions.Single();
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var userId = WellKnownUsers.ThirdExplorer;

        var response = await client.PostAsync($"/api/payment/wallet/{userId}/deposits/{deposit.Id}/reverse", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var stored = await context.Wallets.Include(item => item.Transactions).SingleAsync(item => item.Id == wallet.Id);
        stored.Balance.Should().Be(wallet.Balance);
        stored.Transactions.Should().BeEquivalentTo(wallet.Transactions);
    }

    [Fact]
    public async Task Anonymous_visitor_cannot_reverse_a_deposit()
    {
        var wallet = WalletSeed.ExplorerWalletWithDeposit;
        var deposit = wallet.Transactions.Single();
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/payment/wallet/{wallet.UserId}/deposits/{deposit.Id}/reverse", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_reverse_a_deposit()
    {
        var wallet = WalletSeed.ExplorerWalletWithDeposit;
        var deposit = wallet.Transactions.Single();
        var client = Factory.CreateClientFor(wallet.UserId, "explorer");

        var response = await client.PostAsync($"/api/payment/wallet/{wallet.UserId}/deposits/{deposit.Id}/reverse", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
