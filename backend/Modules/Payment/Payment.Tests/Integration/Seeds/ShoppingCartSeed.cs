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
    }

    public static Cart[] All => [ExplorerCart, OtherExplorerCart, FundedCheckoutCart, FreeCheckoutCart, AlreadyPurchasedCart];
}
