using Payment.Domain.Wallets;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class WalletSeed
{
    public static readonly Wallet EmptyExplorerWallet;
    public static readonly Wallet ExplorerWalletWithReversal;
    public static readonly Wallet ExplorerWalletWithDeposit;

    public static readonly Wallet[] All;

    static WalletSeed()
    {
        EmptyExplorerWallet = new Wallet(WellKnownUsers.Explorer);
        ExplorerWalletWithReversal = new Wallet(WellKnownUsers.SecondExplorer);
        var historyStart = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        ExplorerWalletWithReversal.Deposit(100, WellKnownUsers.Administrator, historyStart);
        var originalDeposit = ExplorerWalletWithReversal.Transactions.Single();
        ExplorerWalletWithReversal.Deposit(20, WellKnownUsers.Administrator, historyStart.AddMinutes(10));
        ExplorerWalletWithReversal.ReverseDeposit(originalDeposit.Id, WellKnownUsers.Administrator, historyStart.AddHours(1));
        ExplorerWalletWithDeposit = new Wallet(WellKnownUsers.FourthExplorer);
        ExplorerWalletWithDeposit.Deposit(40, WellKnownUsers.Administrator, DateTime.UtcNow.AddHours(-1));

        All = [EmptyExplorerWallet, ExplorerWalletWithReversal, ExplorerWalletWithDeposit];
    }
}
