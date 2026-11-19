using FluentAssertions;
using Payment.Domain.Products;
using Payment.Domain.ShoppingCarts;
using Shared.Domain.Exceptions;
using Xunit;

namespace Payment.Tests.Unit.ShoppingCarts;

public class ShoppingCartTests
{
    [Fact]
    public void Adding_a_tour_copies_the_offer_and_increases_the_total()
    {
        var cart = new ShoppingCart(Guid.NewGuid());
        cart.AddItem(CreateProduct(10));
        var product = CreateProduct(25);

        cart.AddItem(product);

        var item = cart.Items.Single(item => item.TourId == product.TourId);
        item.ProductId.Should().Be(product.Id);
        item.Name.Should().Be(product.Name);
        item.PriceInEc.Should().Be(25);
        item.AuthorId.Should().Be(product.AuthorId);
        cart.TotalInEc.Should().Be(35);
    }

    [Fact]
    public void Removing_a_tour_decreases_the_total()
    {
        var cart = new ShoppingCart(Guid.NewGuid());
        var removed = CreateProduct(25);
        var kept = CreateProduct(10);
        cart.AddItem(removed);
        cart.AddItem(kept);

        cart.RemoveItem(removed.TourId);

        cart.Items.Should().ContainSingle().Which.TourId.Should().Be(kept.TourId);
        cart.TotalInEc.Should().Be(10);
    }

    [Fact]
    public void The_same_tour_can_appear_only_once()
    {
        var cart = new ShoppingCart(Guid.NewGuid());
        var product = CreateProduct(25);
        cart.AddItem(product);
        var duplicate = new Product(product.TourId, product.Name, product.AuthorId, 30);

        var add = () => cart.AddItem(duplicate);

        add.Should().Throw<DomainException>();
        cart.Items.Should().ContainSingle();
        cart.TotalInEc.Should().Be(25);
    }

    [Fact]
    public void A_tour_the_buyer_already_owns_cannot_be_added()
    {
        var cart = new ShoppingCart(Guid.NewGuid());
        var purchasing = new CartPurchasing(new CartPricing(new CouponAllocation()));
        var product = CreateProduct(25);

        var add = () => purchasing.AddToCart(cart, product, alreadyPurchased: true);

        add.Should().Throw<DomainException>();
        cart.Items.Should().BeEmpty();
    }

    [Fact]
    public void The_total_can_exceed_the_maximum_price_of_one_tour()
    {
        var cart = new ShoppingCart(Guid.NewGuid());
        cart.AddItem(CreateProduct(int.MaxValue));
        var second = CreateProduct(1);

        cart.AddItem(second);

        cart.TotalInEc.Should().Be((long)int.MaxValue + 1);
    }

    private static Product CreateProduct(int priceInEc)
    {
        return new Product(Guid.NewGuid(), "City walk", Guid.NewGuid(), priceInEc);
    }
}
