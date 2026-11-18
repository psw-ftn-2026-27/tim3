namespace Shared.Tests;

/// <summary>
/// Fixed user IDs that every module's seed data refers to. Feature modules store a
/// UserId as a plain Guid, so these users do not need to exist in the identity schema;
/// tests authenticate as them via <see cref="ExplorerApiFactory.CreateClientFor"/>.
/// </summary>
public static class WellKnownUsers
{
    public static readonly Guid Administrator = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid Explorer = Guid.Parse("00000000-0000-0000-0000-000000000002");
    public static readonly Guid SecondExplorer = Guid.Parse("00000000-0000-0000-0000-000000000003");
    public static readonly Guid ThirdExplorer = Guid.Parse("00000000-0000-0000-0000-000000000004");
    public static readonly Guid FourthExplorer = Guid.Parse("00000000-0000-0000-0000-000000000005");
    public static readonly Guid FifthExplorer = Guid.Parse("00000000-0000-0000-0000-000000000006");
    public static readonly Guid SixthExplorer = Guid.Parse("00000000-0000-0000-0000-000000000007");
    public static readonly Guid SeventhExplorer = Guid.Parse("00000000-0000-0000-0000-000000000008");
    public static readonly Guid EighthExplorer = Guid.Parse("00000000-0000-0000-0000-000000000009");
    public static readonly Guid NinthExplorer = Guid.Parse("00000000-0000-0000-0000-00000000000a");
}
