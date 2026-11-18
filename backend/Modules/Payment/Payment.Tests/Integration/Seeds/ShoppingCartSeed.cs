using Payment.Domain.ShoppingCarts;
using Shared.Tests;
using Cart = Payment.Domain.ShoppingCarts.ShoppingCart;

namespace Payment.Tests.Integration.Seeds;

internal static class ShoppingCartSeed
{
    public static readonly Guid OtherExplorerId = Guid.Parse("20000000-0000-0000-0000-000000000002");
    public static readonly Guid ExplorerWithoutCartId = Guid.Parse("20000000-0000-0000-0000-000000000003");

    public static readonly Cart ExplorerCart;
    public static readonly Cart OtherExplorerCart;
    public static readonly Cart FundedCheckoutCart;
    public static readonly Cart FreeCheckoutCart;
    public static readonly Cart AlreadyPurchasedCart;
    public static readonly Cart ArchivedCheckoutCart;
    public static readonly Cart CartWithWinningAndLosingCoupon;
    public static readonly Cart CartOfBuyerWhoUsedGeneralCoupon;
    public static readonly Cart CartWithGiftCard;
    public static readonly Cart CartWithGiftCardThatExpired;
    public static readonly Cart UnfundedCartWithGiftCard;

    static ShoppingCartSeed()
    {
        ExplorerCart = new Cart(WellKnownUsers.Explorer);
        ExplorerCart.AddItem(ProductSeed.CityWalk);

        OtherExplorerCart = new Cart(OtherExplorerId);
        OtherExplorerCart.AddItem(ProductSeed.MuseumWalk);

        FundedCheckoutCart = new Cart(WellKnownUsers.FourthExplorer);
        FundedCheckoutCart.AddItem(ProductSeed.CityWalk);
        FundedCheckoutCart.AddItem(ProductSeed.FreeWalk);

        FreeCheckoutCart = new Cart(WellKnownUsers.ThirdExplorer);
        FreeCheckoutCart.AddItem(ProductSeed.FreeWalk);

        AlreadyPurchasedCart = new Cart(WellKnownUsers.SecondExplorer);
        AlreadyPurchasedCart.AddItem(ProductSeed.CityWalk);

        ArchivedCheckoutCart = new Cart(WalletSeed.DemoExplorerWalletWithDeposit.UserId);
        ArchivedCheckoutCart.AddItem(ProductSeed.CityWalk);
        ArchivedCheckoutCart.AddItem(ProductSeed.ArchivedWalk);

        // Both coupons fall on City walk; the general one has the larger percent, so the City walk coupon is not applied.
        var purchasing = new CartPurchasing(new CartPricing(new CouponAllocation()));
        var enteredOn = new DateOnly(2026, 1, 1);
        CartWithWinningAndLosingCoupon = new Cart(WellKnownUsers.FifthExplorer);
        CartWithWinningAndLosingCoupon.AddItem(ProductSeed.CityWalk);
        CartWithWinningAndLosingCoupon.AddItem(ProductSeed.MuseumWalk);
        purchasing.EnterCoupon(CartWithWinningAndLosingCoupon, CouponSeed.AuthorCityWalkCoupon, new HashSet<Guid>(), enteredOn);
        purchasing.EnterCoupon(CartWithWinningAndLosingCoupon, CouponSeed.AuthorGeneralCoupon, new HashSet<Guid>(), enteredOn);

        CartOfBuyerWhoUsedGeneralCoupon = new Cart(WellKnownUsers.SixthExplorer);
        CartOfBuyerWhoUsedGeneralCoupon.AddItem(ProductSeed.CityWalk);

        // The card covers City walk in full and part of Museum walk; the rest is paid from the wallet.
        CartWithGiftCard = new Cart(WellKnownUsers.SeventhExplorer);
        CartWithGiftCard.AddItem(ProductSeed.CityWalk);
        CartWithGiftCard.AddItem(ProductSeed.MuseumWalk);
        purchasing.EnterGiftCard(
            CartWithGiftCard, GiftCardSeed.CardInGiftCardBuyerCart, GiftCardSeed.CardInGiftCardBuyerCart.PurchasedAt.AddDays(1));

        CartWithGiftCardThatExpired = new Cart(WellKnownUsers.EighthExplorer);
        CartWithGiftCardThatExpired.AddItem(ProductSeed.CityWalk);
        purchasing.EnterGiftCard(
            CartWithGiftCardThatExpired, GiftCardSeed.CardExpiredAfterEnteringIntoCart,
            GiftCardSeed.CardExpiredAfterEnteringIntoCart.PurchasedAt.AddDays(1));

        // The card leaves more to pay than the wallet holds.
        UnfundedCartWithGiftCard = new Cart(WellKnownUsers.NinthExplorer);
        UnfundedCartWithGiftCard.AddItem(ProductSeed.CityWalk);
        purchasing.EnterGiftCard(
            UnfundedCartWithGiftCard, GiftCardSeed.CardInUnfundedGiftCardBuyerCart,
            GiftCardSeed.CardInUnfundedGiftCardBuyerCart.PurchasedAt.AddDays(1));
    }

    public static Cart[] All =>
    [
        ExplorerCart, OtherExplorerCart, FundedCheckoutCart, FreeCheckoutCart, AlreadyPurchasedCart, ArchivedCheckoutCart,
        CartWithWinningAndLosingCoupon, CartOfBuyerWhoUsedGeneralCoupon, CartWithGiftCard, CartWithGiftCardThatExpired,
        UnfundedCartWithGiftCard
    ];
}
