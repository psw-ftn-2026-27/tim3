using System.Data.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application.TourOffering;
using Payment.Domain.Products;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Persistence.Products;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.TourOffering;

public class TourOfferingConcurrencyTests : BaseIntegrationTest
{
    private const string TourName = "City walk";

    public TourOfferingConcurrencyTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Concurrent_matching_offers_both_succeed_and_save_one_product()
    {
        var tourId = Guid.NewGuid();
        const int priceInEc = 25;

        var results = await CreateConcurrentlyAsync(tourId, priceInEc, priceInEc);

        results.Should().OnlyContain(result => result == null);
        var product = await GetSingleProductAsync(tourId);
        product.Name.Should().Be(TourName);
        product.AuthorId.Should().Be(WellKnownUsers.Explorer);
        product.PriceInEc.Should().Be(priceInEc);
    }

    [Fact]
    public async Task Concurrent_offers_with_different_prices_keep_the_winning_offer()
    {
        var tourId = Guid.NewGuid();
        const int firstPriceInEc = 25;
        const int secondPriceInEc = 40;

        var results = await CreateConcurrentlyAsync(tourId, firstPriceInEc, secondPriceInEc);

        results.Should().ContainSingle(result => result == null);
        results.Should().ContainSingle(result => result is DomainException);
        var product = await GetSingleProductAsync(tourId);
        var winningPriceInEc = results[0] is null ? firstPriceInEc : secondPriceInEc;
        product.PriceInEc.Should().Be(winningPriceInEc);
    }

    private async Task<Exception?[]> CreateConcurrentlyAsync(Guid tourId, int firstPriceInEc, int secondPriceInEc)
    {
        var insertGate = new ProductInsertGate();
        using var firstScope = Factory.Services.CreateScope();
        using var secondScope = Factory.Services.CreateScope();
        using var firstContext = CreateContext(firstScope.ServiceProvider, insertGate);
        using var secondContext = CreateContext(secondScope.ServiceProvider, insertGate);
        var firstOffer = new TourOfferingService(new ProductRepository(firstContext), firstContext);
        var secondOffer = new TourOfferingService(new ProductRepository(secondContext), secondContext);

        var firstResult = Record.ExceptionAsync(() =>
            firstOffer.CreateProductAsync(tourId, TourName, WellKnownUsers.Explorer, firstPriceInEc));
        var secondResult = Record.ExceptionAsync(() =>
            secondOffer.CreateProductAsync(tourId, TourName, WellKnownUsers.Explorer, secondPriceInEc));
        var results = await Task.WhenAll(firstResult, secondResult);

        insertGate.ReachedByBothAttempts.Should().BeTrue("both attempts must reach the insert before either saves");
        return results;
    }

    private static PaymentDbContext CreateContext(IServiceProvider services, ProductInsertGate insertGate)
    {
        var options = services.GetRequiredService<DbContextOptions<PaymentDbContext>>();
        var interceptedOptions = new DbContextOptionsBuilder<PaymentDbContext>(options)
            .AddInterceptors(insertGate)
            .Options;
        return new PaymentDbContext(interceptedOptions);
    }

    private async Task<Product> GetSingleProductAsync(Guid tourId)
    {
        using var context = Factory.CreateContext<PaymentDbContext>();
        var products = await context.Products.AsNoTracking().Where(product => product.TourId == tourId).ToListAsync();
        return products.Should().ContainSingle().Which;
    }

    private sealed class ProductInsertGate : DbCommandInterceptor
    {
        private readonly TaskCompletionSource _bothInsertsReady = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _insertCount;

        public bool ReachedByBothAttempts => _bothInsertsReady.Task.IsCompletedSuccessfully;

        public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            if (command.CommandText.StartsWith("INSERT INTO payment.\"Products\"", StringComparison.Ordinal))
            {
                if (Interlocked.Increment(ref _insertCount) == 2)
                {
                    _bothInsertsReady.SetResult();
                }

                await _bothInsertsReady.Task.WaitAsync(TimeSpan.FromSeconds(15));
            }

            return result;
        }
    }
}
