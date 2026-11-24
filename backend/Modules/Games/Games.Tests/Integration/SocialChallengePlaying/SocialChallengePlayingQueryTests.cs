using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.SocialChallengePlaying;
using Games.Domain.SocialChallengeExecutions;
using Games.Domain.SocialChallenges;
using Games.Tests.Integration.Seeds;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengePlaying;

public class SocialChallengePlayingQueryTests : BaseIntegrationTest
{
    public SocialChallengePlayingQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_lists_only_published_social_challenges_with_how_many_times_that_explorer_solved_each()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var publishedIds = SocialChallengeSeed.All
            .Where(socialChallenge => socialChallenge.Status == SocialChallengeStatus.Published)
            .Select(socialChallenge => socialChallenge.Id);
        var noonOnTheSquare = SocialChallengeSeed.PublishedNoonOnTheSquare;
        var explorerSolvedCount = SocialChallengeExecutionSeed.All.Count(execution =>
            execution.ChallengeId == noonOnTheSquare.Id
            && execution.Status == SocialChallengeExecutionStatus.Solved
            && execution.Participants.Any(participant => participant.ExplorerId == WellKnownUsers.Explorer));

        var socialChallenges = await client.GetFromJsonAsync<PageResult<PublishedSocialChallengeDto>>(
            "/api/games/social-challenges/published", JsonOptions);

        socialChallenges!.Items.Select(socialChallenge => socialChallenge.Id).Should().BeEquivalentTo(publishedIds);
        socialChallenges.Items.Single(socialChallenge => socialChallenge.Id == noonOnTheSquare.Id)
            .SolvedCount.Should().Be(explorerSolvedCount);
    }
}
