using System.Text.Json;
using System.Text.Json.Serialization;
using Identity.Infrastructure;
using Identity.Tests.Integration.Seeds;
using Shared.Tests;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration;

public sealed class SocialApiFactory : ExplorerApiFactory;

[CollectionDefinition("Integration")]
public sealed class IntegrationCollection : ICollectionFixture<SocialApiFactory>;

[Collection("Integration")]
public abstract class BaseIntegrationTest
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected readonly SocialApiFactory Factory;

    protected BaseIntegrationTest(SocialApiFactory factory)
    {
        Factory = factory;
        Factory.Reseed<SocialDbContext>(SocialSeed.All);
        Factory.Reseed<IdentityModuleDbContext>(IdentitySeed.All);
    }
}
