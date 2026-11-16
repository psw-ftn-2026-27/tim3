using FluentAssertions;
using Payment.Domain.Products;
using Shared.Domain.Exceptions;
using Xunit;

namespace Payment.Tests.Unit.Products;

public class ProductTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(25)]
    public void A_tour_can_be_offered_for_free_or_at_a_positive_price(int priceInEc)
    {
        var tourId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        const string name = "City walk";

        var product = new Product(tourId, name, authorId, priceInEc);

        product.TourId.Should().Be(tourId);
        product.Name.Should().Be(name);
        product.AuthorId.Should().Be(authorId);
        product.PriceInEc.Should().Be(priceInEc);
    }

    [Fact]
    public void A_tour_cannot_be_offered_at_a_negative_price()
    {
        var tourId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        var offerTour = () => new Product(tourId, "City walk", authorId, -1);

        offerTour.Should().Throw<DomainException>();
    }

    [Fact]
    public void The_same_offer_can_be_retried()
    {
        var authorId = Guid.NewGuid();
        var product = new Product(Guid.NewGuid(), "City walk", authorId, 25);

        var retry = () => product.EnsureSameOffer("City walk", authorId, 25);

        retry.Should().NotThrow();
    }

    [Theory]
    [InlineData("City walk", true, 0)]
    [InlineData("City walk", true, 30)]
    [InlineData("City tour", true, 25)]
    [InlineData("City walk", false, 25)]
    public void A_retry_cannot_change_an_existing_offer(string name, bool sameAuthor, int priceInEc)
    {
        var authorId = Guid.NewGuid();
        var retryAuthorId = sameAuthor ? authorId : Guid.NewGuid();
        var product = new Product(Guid.NewGuid(), "City walk", authorId, 25);

        var retry = () => product.EnsureSameOffer(name, retryAuthorId, priceInEc);

        retry.Should().Throw<DomainException>();
        product.Name.Should().Be("City walk");
        product.AuthorId.Should().Be(authorId);
        product.PriceInEc.Should().Be(25);
    }
}
