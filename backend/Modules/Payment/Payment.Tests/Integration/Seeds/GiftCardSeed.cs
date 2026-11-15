using Payment.Domain.GiftCards;

namespace Payment.Tests.Integration.Seeds;

internal static class GiftCardSeed
{
    // Identity seeds explorer1, explorer2 and explorer3 in the Development test host.
    public static readonly Guid DemoExplorerId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public static readonly Guid SecondDemoExplorerId = Guid.Parse("20000000-0000-0000-0000-000000000002");
    public static readonly Guid ThirdDemoExplorerId = Guid.Parse("20000000-0000-0000-0000-000000000003");

    public static readonly GiftCard DemoExplorerCardForSecondDemoExplorer;
    public static readonly GiftCard DemoExplorerCardForThirdDemoExplorer;
    public static readonly GiftCard SecondDemoExplorerCardForDemoExplorer;

    public static readonly GiftCard[] All;

    static GiftCardSeed()
    {
        var purchasedAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        DemoExplorerCardForSecondDemoExplorer = new GiftCard(DemoExplorerId, SecondDemoExplorerId, 20, "Happy birthday", purchasedAt);
        DemoExplorerCardForThirdDemoExplorer = new GiftCard(DemoExplorerId, ThirdDemoExplorerId, 15, null, purchasedAt.AddDays(1));
        SecondDemoExplorerCardForDemoExplorer = new GiftCard(SecondDemoExplorerId, DemoExplorerId, 30, null, purchasedAt);

        All = [DemoExplorerCardForSecondDemoExplorer, DemoExplorerCardForThirdDemoExplorer, SecondDemoExplorerCardForDemoExplorer];
    }
}
