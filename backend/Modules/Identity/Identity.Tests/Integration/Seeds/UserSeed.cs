using Identity.Core;
using Microsoft.AspNetCore.Identity;
using Shared.Tests;

namespace Identity.Tests.Integration.Seeds;

/// <summary>
/// Public so that other modules' integration tests can seed users whose names they show.
/// Unlike feature-module seeds, these rows cannot come from a domain constructor:
/// Reseed inserts them straight into the database, bypassing UserManager, so the
/// normalized columns and the password hash must be filled in here.
/// </summary>
public static class UserSeed
{
    public const string Password = "SuperSecret1!";

    private static readonly PasswordHasher<ApplicationUser> Hasher = new();

    public static readonly ApplicationUser Explorer = new()
    {
        Id = WellKnownUsers.Explorer,
        UserName = "explorer@test.com",
        NormalizedUserName = "EXPLORER@TEST.COM",
        Email = "explorer@test.com",
        NormalizedEmail = "EXPLORER@TEST.COM",
        PasswordHash = Hasher.HashPassword(null!, Password),
        SecurityStamp = "5f4a2f3e-8b1c-4d6e-9a0b-7c2d1e3f4a5b"
    };

    public static readonly IdentityUserRole<Guid> ExplorerRole = new() { UserId = WellKnownUsers.Explorer, RoleId = RoleSeed.Explorer.Id };

    public static readonly ApplicationUser SecondExplorer = new()
    {
        Id = WellKnownUsers.SecondExplorer,
        UserName = "second.explorer@test.com",
        NormalizedUserName = "SECOND.EXPLORER@TEST.COM",
        Email = "second.explorer@test.com",
        NormalizedEmail = "SECOND.EXPLORER@TEST.COM",
        PasswordHash = Hasher.HashPassword(null!, Password),
        SecurityStamp = "8c3e1b7a-2d4f-4e9a-b6c1-0f5d7e2a9b3c"
    };

    public static readonly IdentityUserRole<Guid> SecondExplorerRole = new() { UserId = WellKnownUsers.SecondExplorer, RoleId = RoleSeed.Explorer.Id };

    public static object[] All => [Explorer, ExplorerRole, SecondExplorer, SecondExplorerRole];
}
