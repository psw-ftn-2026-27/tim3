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

    public static readonly ApplicationUser ThirdExplorer = new()
    {
        Id = WellKnownUsers.ThirdExplorer,
        UserName = "third.explorer@test.com",
        NormalizedUserName = "THIRD.EXPLORER@TEST.COM",
        Email = "third.explorer@test.com",
        NormalizedEmail = "THIRD.EXPLORER@TEST.COM",
        PasswordHash = Hasher.HashPassword(null!, Password),
        SecurityStamp = "2a7d9e4c-6b1f-4c8a-9e3d-5f0b8c1a7d26"
    };

    public static readonly IdentityUserRole<Guid> ThirdExplorerRole = new() { UserId = WellKnownUsers.ThirdExplorer, RoleId = RoleSeed.Explorer.Id };

    public static readonly ApplicationUser FourthExplorer = new()
    {
        Id = WellKnownUsers.FourthExplorer,
        UserName = "fourth.explorer@test.com",
        NormalizedUserName = "FOURTH.EXPLORER@TEST.COM",
        Email = "fourth.explorer@test.com",
        NormalizedEmail = "FOURTH.EXPLORER@TEST.COM",
        PasswordHash = Hasher.HashPassword(null!, Password),
        SecurityStamp = "b4e1c7f9-3a2d-4f6b-8c5e-1d9a0e7b3f42"
    };

    public static readonly IdentityUserRole<Guid> FourthExplorerRole = new() { UserId = WellKnownUsers.FourthExplorer, RoleId = RoleSeed.Explorer.Id };

    public static object[] All => [Explorer, ExplorerRole, SecondExplorer, SecondExplorerRole, ThirdExplorer, ThirdExplorerRole, FourthExplorer, FourthExplorerRole];
}
