using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.SocialChallengeAuthoring;
using Games.Tests.Integration.Seeds;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengeAuthoring;

public class SocialChallengeAuthoringQueryTests : BaseIntegrationTest
{
    public SocialChallengeAuthoringQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_sees_all_seeded_social_challenges()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var challenges = await client.GetFromJsonAsync<PageResult<SocialChallengeDto>>("/api/games/social-challenges", JsonOptions);

        challenges!.TotalCount.Should().Be(SocialChallengeSeed.All.Length);
        challenges.Items.Select(challenge => challenge.Id).Should().BeEquivalentTo(SocialChallengeSeed.All.Select(challenge => challenge.Id));
    }

    [Fact]
    public async Task Administrator_fetches_a_social_challenge_to_edit()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var seeded = SocialChallengeSeed.MidnightOnTheBridge;

        var challenge = await client.GetFromJsonAsync<SocialChallengeDto>($"/api/games/social-challenges/{seeded.Id}", JsonOptions);

        challenge.Should().Be(new SocialChallengeDto(seeded.Id, seeded.Name, seeded.Status, seeded.ActivePeriod.Start,
            seeded.ActivePeriod.End, seeded.PeopleRequired, seeded.GatheringRadius, seeded.ExperienceGained, seeded.EndMessage));
    }
}
