using AutoMapper;
using Games.Application.SocialChallengeAuthoring;
using Games.Domain.SocialChallenges;

namespace Games.Application;

public sealed class GamesMapperProfile : Profile
{
    public GamesMapperProfile()
    {
        CreateMap<SocialChallenge, SocialChallengeDto>()
            .ForCtorParam(nameof(SocialChallengeDto.Latitude), options => options.MapFrom(socialChallenge =>
                socialChallenge.Location == null ? (double?)null : socialChallenge.Location.Latitude))
            .ForCtorParam(nameof(SocialChallengeDto.Longitude), options => options.MapFrom(socialChallenge =>
                socialChallenge.Location == null ? (double?)null : socialChallenge.Location.Longitude));
    }
}
