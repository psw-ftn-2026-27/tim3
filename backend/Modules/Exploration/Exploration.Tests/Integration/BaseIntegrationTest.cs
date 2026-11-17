using System.Text.Json;
using System.Text.Json.Serialization;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Tests;
using Xunit;
using PaymentUnitOfWork = Payment.Application.IUnitOfWork;

namespace Exploration.Tests.Integration;

public sealed class ExplorationApiFactory : ExplorerApiFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        // Reuse the Payment context for seeds and assertions without exposing its concrete type.
        builder.ConfigureTestServices(services => services.AddScoped<DbContext>(provider =>
            (DbContext)provider.GetRequiredService<PaymentUnitOfWork>()));
    }
}

[CollectionDefinition("Integration")]
public sealed class IntegrationCollection : ICollectionFixture<ExplorationApiFactory>;

[Collection("Integration")]
public abstract class BaseIntegrationTest
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected readonly ExplorationApiFactory Factory;

    protected BaseIntegrationTest(ExplorationApiFactory factory)
    {
        Factory = factory;
        Factory.Reseed<ExplorationDbContext>(ExplorationSeed.All);
        Factory.Reseed<DbContext>([.. ProductSeed.All, .. TourPurchaseTokenSeed.All]);
    }
}
