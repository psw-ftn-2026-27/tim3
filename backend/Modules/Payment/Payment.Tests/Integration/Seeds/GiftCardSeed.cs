using Payment.Domain.GiftCards;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class GiftCardSeed
{
    // Identity seeds admin1, explorer1, explorer2 and explorer3 in the Development test host.
    public static readonly Guid DemoAdministratorId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid DemoExplorerId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public static readonly Guid SecondDemoExplorerId = Guid.Parse("20000000-0000-0000-0000-000000000002");
    public static readonly Guid ThirdDemoExplorerId = Guid.Parse("20000000-0000-0000-0000-000000000003");

    public static readonly GiftCard DemoExplorerCardForSecondDemoExplorer;
    public static readonly GiftCard DemoExplorerCardForThirdDemoExplorer;
    public static readonly GiftCard SecondDemoExplorerCardForDemoExplorer;
    public static readonly GiftCard ThirdDemoExplorerCardForDemoExplorer;
    public static readonly GiftCard CardInGiftCardBuyerCart;
    public static readonly GiftCard RedeemedCardOfGiftCardBuyer;
    public static readonly GiftCard ExpiredCardOfGiftCardBuyer;
    public static readonly GiftCard CardExpiredAfterEnteringIntoCart;
    public static readonly GiftCard CardInUnfundedGiftCardBuyerCart;

    public static readonly GiftCard[] All;

    static GiftCardSeed()
    {
        var purchasedAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        DemoExplorerCardForSecondDemoExplorer = new GiftCard(DemoExplorerId, SecondDemoExplorerId, 20, "Happy birthday", purchasedAt);
        DemoExplorerCardForThirdDemoExplorer = new GiftCard(DemoExplorerId, ThirdDemoExplorerId, 15, null, purchasedAt.AddDays(1));
        SecondDemoExplorerCardForDemoExplorer = new GiftCard(SecondDemoExplorerId, DemoExplorerId, 30, null, purchasedAt);
        ThirdDemoExplorerCardForDemoExplorer = new GiftCard(ThirdDemoExplorerId, DemoExplorerId, 50, "Enjoy the tours", purchasedAt.AddDays(2));

        var longAgo = new DateTime(2020, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        CardInGiftCardBuyerCart = new GiftCard(WellKnownUsers.SecondExplorer, WellKnownUsers.SeventhExplorer, 30, null, purchasedAt);
        RedeemedCardOfGiftCardBuyer = new GiftCard(WellKnownUsers.SecondExplorer, WellKnownUsers.SeventhExplorer, 20, null, purchasedAt);
        RedeemedCardOfGiftCardBuyer.Redeem(WellKnownUsers.SeventhExplorer, purchasedAt.AddMonths(1));
        ExpiredCardOfGiftCardBuyer = new GiftCard(WellKnownUsers.SecondExplorer, WellKnownUsers.SeventhExplorer, 15, null, longAgo);
        CardExpiredAfterEnteringIntoCart = new GiftCard(WellKnownUsers.SecondExplorer, WellKnownUsers.EighthExplorer, 10, null, longAgo);
        CardInUnfundedGiftCardBuyerCart = new GiftCard(WellKnownUsers.SecondExplorer, WellKnownUsers.NinthExplorer, 10, null, purchasedAt);

        All =
        [
            DemoExplorerCardForSecondDemoExplorer,
            DemoExplorerCardForThirdDemoExplorer,
            SecondDemoExplorerCardForDemoExplorer,
            ThirdDemoExplorerCardForDemoExplorer,
            CardInGiftCardBuyerCart,
            RedeemedCardOfGiftCardBuyer,
            ExpiredCardOfGiftCardBuyer,
            CardExpiredAfterEnteringIntoCart,
            CardInUnfundedGiftCardBuyerCart
        ];
    }
}
