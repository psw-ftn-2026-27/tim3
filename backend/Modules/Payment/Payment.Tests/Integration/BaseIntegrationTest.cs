using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;
using ExplorationUnitOfWork = Exploration.Application.IUnitOfWork;

namespace Payment.Tests.Integration;

public sealed class PaymentApiFactory : ExplorerApiFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureTestServices(services => services.AddScoped<DbContext>(provider =>
            (DbContext)provider.GetRequiredService<ExplorationUnitOfWork>()));
    }
}

[CollectionDefinition("Integration")]
public sealed class IntegrationCollection : ICollectionFixture<PaymentApiFactory>;

[Collection("Integration")]
public abstract class BaseIntegrationTest
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected readonly PaymentApiFactory Factory;

    protected BaseIntegrationTest(PaymentApiFactory factory)
    {
        Factory = factory;
        Factory.Reseed<PaymentDbContext>(PaymentSeed.All);
    }
}
