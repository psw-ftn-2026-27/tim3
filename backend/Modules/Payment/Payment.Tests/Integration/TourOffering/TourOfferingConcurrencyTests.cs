using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourAuthoring;
using Exploration.Domain.Tours;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.TourOffering;

public class TourOfferingConcurrencyTests : BaseIntegrationTest
{
    public TourOfferingConcurrencyTests(PaymentApiFactory factory) : base(factory)
    {
        Factory.Reseed<DbContext>(CheckoutTourSeed.All);
    }

    [Theory]
    [InlineData(25)]
    [InlineData(40)]
    public async Task Concurrent_publications_keep_a_single_product_with_the_successful_price(int secondPrice)
    {
        var tour = CheckoutTourSeed.PublishableWalk;
        using var firstClient = Factory.CreateClientFor(tour.AuthorId, "explorer");
        using var secondClient = Factory.CreateClientFor(tour.AuthorId, "explorer");
        var url = $"/api/exploration/tours/{tour.Id}/publish";
        int[] prices = [25, secondPrice];

        var responses = await Task.WhenAll(
            firstClient.PostAsJsonAsync(url, new PublishTourDto(false, prices[0]), JsonOptions),
            secondClient.PostAsJsonAsync(url, new PublishTourDto(false, prices[1]), JsonOptions));

        responses.Should().Contain(response => response.StatusCode == HttpStatusCode.NoContent);
        responses.Should().OnlyContain(response =>
            response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var products = await context.Products.Where(product => product.TourId == tour.Id).ToListAsync();
        var product = products.Should().ContainSingle().Which;
        product.Name.Should().Be(tour.Name);
        product.AuthorId.Should().Be(tour.AuthorId);
        for (var index = 0; index < responses.Length; index++)
        {
            if (responses[index].IsSuccessStatusCode)
            {
                product.PriceInEc.Should().Be(prices[index]);
            }
        }
        using var exploration = Factory.CreateContext<DbContext>();
        (await exploration.Set<Tour>().SingleAsync(row => row.Id == tour.Id)).Status.Should().Be(TourStatus.Published);
    }
}
