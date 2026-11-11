using Payment.Domain.Wallets;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class WalletSeed
{
    public static readonly Wallet EmptyExplorerWallet;
    public static readonly Wallet EmptySecondExplorerWallet;
    public static readonly Wallet ExplorerWalletWithDeposit;

    public static readonly Wallet[] All;

    static WalletSeed()
    {
        EmptyExplorerWallet = new Wallet(WellKnownUsers.Explorer);
        EmptySecondExplorerWallet = new Wallet(WellKnownUsers.SecondExplorer);
        ExplorerWalletWithDeposit = new Wallet(WellKnownUsers.FourthExplorer);
        ExplorerWalletWithDeposit.Deposit(40, WellKnownUsers.Administrator,
            new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc));

        All = [EmptyExplorerWallet, EmptySecondExplorerWallet, ExplorerWalletWithDeposit];
    }
}
