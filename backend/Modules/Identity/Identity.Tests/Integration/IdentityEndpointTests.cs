using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Identity.Core.DTOs;
using Identity.Infrastructure;
using Identity.Tests.Integration.Seeds;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Identity.Tests.Integration;

public sealed class IdentityEndpointTests : BaseIntegrationTest
{
    public IdentityEndpointTests(IdentityApiFactory factory) : base(factory) { }

    [Fact]
    public async Task New_explorer_registers_with_an_empty_wallet()
    {
        var client = Factory.CreateClient();
        var request = new RegisterDto("new-explorer@test.com", "BrandNewSecret1!");

        var response = await client.PostAsJsonAsync("/api/identity/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AccessTokenDto>();
        var token = new JwtSecurityTokenHandler().ReadJwtToken(body!.AccessToken);
        token.Claims.Should().Contain(claim => claim.Type == ClaimTypes.Role && claim.Value == "explorer");
        var userId = Guid.Parse(token.Subject);

        using var context = Factory.CreateContext<IdentityModuleDbContext>();
        var balance = await context.Database.SqlQuery<int>($"""
            SELECT "Balance" AS "Value" FROM payment."Wallets" WHERE "UserId" = {userId}
            """).SingleAsync();
        balance.Should().Be(0);
        var hasTransactions = await context.Database.SqlQuery<Guid>($"""
            SELECT transactions."Id" AS "Value"
            FROM payment."WalletTransactions" AS transactions
            JOIN payment."Wallets" AS wallets ON wallets."Id" = transactions."WalletId"
            WHERE wallets."UserId" = {userId}
            """).AnyAsync();
        hasTransactions.Should().BeFalse();
    }

    [Fact]
    public async Task Registration_keeps_the_account_but_returns_no_token_when_wallet_creation_fails()
    {
        var client = Factory.CreateClient();
        var request = new RegisterDto("wallet-failure@test.com", "BrandNewSecret1!");
        await SetWalletInsertFailureAsync(enabled: true);

        HttpResponseMessage response;
        try
        {
            response = await client.PostAsJsonAsync("/api/identity/register", request);
        }
        finally
        {
            await SetWalletInsertFailureAsync(enabled: false);
        }

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("accessToken", out _).Should().BeFalse();
        using var context = Factory.CreateContext<IdentityModuleDbContext>();
        var user = await context.Users.AsNoTracking().SingleAsync(user => user.Email == request.Email);
        var hasWallet = await context.Database.SqlQuery<Guid>($"""
            SELECT "Id" AS "Value" FROM payment."Wallets" WHERE "UserId" = {user.Id}
            """).AnyAsync();
        hasWallet.Should().BeFalse();
    }

    [Fact]
    public async Task Email_can_be_registered_only_once()
    {
        var client = Factory.CreateClient();
        var request = new RegisterDto(UserSeed.Explorer.Email!, "BrandNewSecret1!");

        var response = await client.PostAsJsonAsync("/api/identity/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task User_logs_in_with_valid_credentials()
    {
        var client = Factory.CreateClient();
        var request = new LoginDto(UserSeed.Explorer.Email!, UserSeed.Password);

        var response = await client.PostAsJsonAsync("/api/identity/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AccessTokenDto>();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task User_cannot_log_in_with_a_wrong_password()
    {
        var client = Factory.CreateClient();
        var request = new LoginDto(UserSeed.Explorer.Email!, "WrongPassword1!");

        var response = await client.PostAsJsonAsync("/api/identity/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task SetWalletInsertFailureAsync(bool enabled)
    {
        using var context = Factory.CreateContext<IdentityModuleDbContext>();
        await context.Database.ExecuteSqlRawAsync(enabled
            ? """ALTER TABLE payment."Wallets" ADD CONSTRAINT "Test_RejectWalletCreation" CHECK (false) NOT VALID"""
            : """ALTER TABLE payment."Wallets" DROP CONSTRAINT "Test_RejectWalletCreation" """);
    }
}
