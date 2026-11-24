using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Payment.Application;
using Payment.Domain.TourPurchases;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Domain.Exceptions;
using Xunit;

namespace Payment.Tests.Integration.Persistence;

// Exercise PostgreSQL and the unit of work directly. HTTP rejects these duplicates
// before persistence unless requests race, so HTTP alone cannot guarantee either constraint is hit.
public sealed class PurchaseConstraintsTests : BaseIntegrationTest
{
    public PurchaseConstraintsTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task A_duplicate_order_item_is_rejected_by_the_unit_of_work()
    {
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.SingleAsync(row => row.Id == ShoppingCartSeed.ExplorerCart.Id);
        var item = cart.Items.Single();

        // Reinsert just the existing item without changing the parent cart's revision.
        context.Entry(item).State = EntityState.Added;
        Func<Task> save = () => ((IUnitOfWork)context).SaveChangesAsync();

        await save.Should().ThrowExactlyAsync<DomainException>();
    }

    [Fact]
    public async Task A_second_token_for_the_same_buyer_and_tour_is_rejected_by_the_unit_of_work()
    {
        var existing = TourPurchaseTokenSeed.ExplorerMuseumWalk;
        using var context = Factory.CreateContext<PaymentDbContext>();
        context.TourPurchaseTokens.Add(new TourPurchaseToken(existing.BuyerId, existing.TourId, existing.PurchasedAt));
        Func<Task> save = () => ((IUnitOfWork)context).SaveChangesAsync();

        await save.Should().ThrowExactlyAsync<DomainException>();
    }
}
