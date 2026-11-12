using FluentAssertions;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.Wallets;

public class WalletTests
{
    private static readonly DateTime DepositTime = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void A_new_deposit_adds_to_the_balance_and_keeps_the_previous_deposit()
    {
        var wallet = CreateWalletWithDeposit(40);
        var previousDeposit = wallet.Transactions.Single();
        var nextDepositTime = DepositTime.AddMinutes(1);

        wallet.Deposit(60, WellKnownUsers.Administrator, nextDepositTime);

        wallet.Balance.Should().Be(100);
        wallet.Transactions.Should().HaveCount(2).And.Contain(previousDeposit);
        var newDeposit = wallet.Transactions.Single(transaction => transaction.Id != previousDeposit.Id);
        newDeposit.Id.Should().NotBeEmpty();
        newDeposit.Amount.Should().Be(60);
        newDeposit.Kind.Should().Be(WalletTransactionKind.Deposit);
        newDeposit.AdministratorId.Should().Be(WellKnownUsers.Administrator);
        newDeposit.OccurredAt.Should().Be(nextDepositTime);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void Rejected_deposits_leave_the_balance_and_history_intact(int amount)
    {
        var wallet = CreateWalletWithDeposit(40);
        var previousDeposit = wallet.Transactions.Single();

        var deposit = () => wallet.Deposit(amount, WellKnownUsers.Administrator, DepositTime);

        deposit.Should().Throw<DomainException>();
        wallet.Balance.Should().Be(40);
        wallet.Transactions.Should().ContainSingle().Which.Should().Be(previousDeposit);
    }

    [Fact]
    public void A_deposit_can_be_reversed_exactly_twenty_four_hours_later()
    {
        var wallet = CreateWalletWithDeposit(40);
        var deposit = wallet.Transactions.Single();

        wallet.ReverseDeposit(deposit.Id, WellKnownUsers.Administrator, DepositTime.AddHours(24));

        wallet.Balance.Should().Be(0);
        wallet.Transactions.Should().Contain(deposit);
        wallet.Transactions.Should().ContainSingle(transaction =>
            transaction.Kind == WalletTransactionKind.Reversal && transaction.OriginalDepositId == deposit.Id);
    }

    [Fact]
    public void A_deposit_cannot_be_reversed_after_twenty_four_hours()
    {
        var wallet = CreateWalletWithDeposit(40);
        var deposit = wallet.Transactions.Single();
        var tooLate = DepositTime.AddHours(24).AddTicks(1);

        var reverse = () => wallet.ReverseDeposit(deposit.Id, WellKnownUsers.Administrator, tooLate);

        reverse.Should().Throw<DomainException>();
        wallet.Balance.Should().Be(40);
        wallet.Transactions.Should().ContainSingle().Which.Should().Be(deposit);
    }

    [Fact]
    public void A_deposit_cannot_be_reversed_twice()
    {
        var wallet = CreateWalletWithDeposit(40);
        var deposit = wallet.Transactions.Single();
        var reversedAt = DepositTime.AddHours(1);
        wallet.ReverseDeposit(deposit.Id, WellKnownUsers.Administrator, reversedAt);
        wallet.Deposit(50, WellKnownUsers.Administrator, reversedAt);
        var history = wallet.Transactions.ToList();

        var reverse = () => wallet.ReverseDeposit(deposit.Id, WellKnownUsers.Administrator, reversedAt);

        reverse.Should().Throw<DomainException>();
        wallet.Balance.Should().Be(50);
        wallet.Transactions.Should().Equal(history);
    }

    [Fact]
    public void A_deposit_outside_the_wallet_cannot_be_reversed()
    {
        var wallet = CreateWalletWithDeposit(40);
        var deposit = wallet.Transactions.Single();
        var otherWallet = new Wallet(WellKnownUsers.SecondExplorer);
        otherWallet.Deposit(40, WellKnownUsers.Administrator, DepositTime);
        var otherDeposit = otherWallet.Transactions.Single();

        var reverse = () => wallet.ReverseDeposit(otherDeposit.Id, WellKnownUsers.Administrator, DepositTime.AddHours(1));

        reverse.Should().Throw<NotFoundException>();
        wallet.Balance.Should().Be(40);
        wallet.Transactions.Should().ContainSingle().Which.Should().Be(deposit);
    }

    private static Wallet CreateWalletWithDeposit(int amount)
    {
        var wallet = new Wallet(WellKnownUsers.Explorer);
        wallet.Deposit(amount, WellKnownUsers.Administrator, DepositTime);
        return wallet;
    }
}
