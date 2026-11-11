namespace Identity.Tests.Integration.Seeds;

public static class IdentitySeed
{
    public static object[] All => [.. RoleSeed.All, .. UserSeed.All];
}
