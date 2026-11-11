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

    private static Wallet CreateWalletWithDeposit(int amount)
    {
        var wallet = new Wallet(WellKnownUsers.Explorer);
        wallet.Deposit(amount, WellKnownUsers.Administrator, DepositTime);
        return wallet;
    }
}
