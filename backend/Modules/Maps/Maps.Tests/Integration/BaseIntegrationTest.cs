using System.Text.Json;
using System.Text.Json.Serialization;
using Maps.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shared.Tests;
using Xunit;

namespace Maps.Tests.Integration;

public sealed class MapsApiFactory : ExplorerApiFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IRoutingService>();
            services.AddSingleton<IRoutingService, FakeRoutingService>();
        });
    }
}

[CollectionDefinition("Integration")]
public sealed class IntegrationCollection : ICollectionFixture<MapsApiFactory>;

[Collection("Integration")]
public abstract class BaseIntegrationTest
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected readonly MapsApiFactory Factory;

    protected BaseIntegrationTest(MapsApiFactory factory)
    {
        Factory = factory;
    }
}
