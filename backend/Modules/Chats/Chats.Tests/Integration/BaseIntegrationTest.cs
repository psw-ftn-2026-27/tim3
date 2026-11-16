using System.Text.Json;
using System.Text.Json.Serialization;
using Chats.Infrastructure.Persistence;
using Chats.Tests.Integration.Seeds;
using Identity.Infrastructure;
using Identity.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Chats.Tests.Integration;

public sealed class ChatsApiFactory : ExplorerApiFactory;

[CollectionDefinition("Integration")]
public sealed class IntegrationCollection : ICollectionFixture<ChatsApiFactory>;

[Collection("Integration")]
public abstract class BaseIntegrationTest
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected readonly ChatsApiFactory Factory;

    protected BaseIntegrationTest(ChatsApiFactory factory)
    {
        Factory = factory;
        Factory.Reseed<ChatsDbContext>(ChatsSeed.All);
        Factory.Reseed<IdentityModuleDbContext>(IdentitySeed.All);
    }
}
