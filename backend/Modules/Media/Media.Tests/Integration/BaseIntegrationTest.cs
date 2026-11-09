using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Shared.Tests;
using Xunit;

namespace Media.Tests.Integration;

public sealed class MediaApiFactory : ExplorerApiFactory
{
    public string StoragePath { get; }

    public MediaApiFactory()
    {
        StoragePath = Path.Combine(Path.GetTempPath(), "explorer-media-tests", Guid.NewGuid().ToString("N"));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSetting("Media:StoragePath", StoragePath);
    }
}

[CollectionDefinition("Integration")]
public sealed class IntegrationCollection : ICollectionFixture<MediaApiFactory>;

[Collection("Integration")]
public abstract class BaseIntegrationTest
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected readonly MediaApiFactory Factory;

    protected BaseIntegrationTest(MediaApiFactory factory)
    {
        Factory = factory;
    }
}
