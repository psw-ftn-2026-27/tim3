using System.Net.Http.Json;
using FluentAssertions;
using Shared.Domain;
using Shared.Tests;
using Social.Application.ClubManagement;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubManagement;

public class ClubManagementQueryTests : BaseIntegrationTest
{
    public ClubManagementQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Every_user_sees_all_seeded_clubs()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var clubs = await client.GetFromJsonAsync<PageResult<ClubDto>>("/api/social/clubs", JsonOptions);

        clubs!.TotalCount.Should().Be(ClubSeed.All.Length);
        clubs.Items.Select(club => club.Id).Should().BeEquivalentTo(ClubSeed.All.Select(club => club.Id));
        clubs.Items.Should().Contain(club => club.Id == ClubSeed.FortressLovers.Id
            && club.OwnerId == WellKnownUsers.SecondExplorer
            && club.Images.Count == ClubSeed.FortressLovers.Images.Count);
    }
}
