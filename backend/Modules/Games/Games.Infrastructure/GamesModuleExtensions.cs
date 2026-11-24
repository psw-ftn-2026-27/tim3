using Games.Application;
using Games.Application.LocationChallengeAuthoring;
using Games.Application.LocationChallengeParticipations;
using Games.Application.LocationChallenges;
using Games.Application.LocationChallengeSolving;
using Games.Application.QuizAuthoring;
using Games.Application.QuizLifecycle;
using Games.Application.QuizParticipations;
using Games.Application.QuizPlaying;
using Games.Application.Quizzes;
using Games.Application.SocialChallengeAuthoring;
using Games.Application.SocialChallengeExecutions;
using Games.Application.SocialChallengeLifecycle;
using Games.Application.SocialChallengePlaying;
using Games.Application.SocialChallenges;
using Games.Domain.LocationChallenges;
using Games.Domain.QuizParticipations;
using Games.Domain.SocialChallengeExecutions;
using Games.Infrastructure.Persistence;
using Games.Infrastructure.Persistence.LocationChallengeParticipations;
using Games.Infrastructure.Persistence.LocationChallenges;
using Games.Infrastructure.Persistence.QuizParticipations;
using Games.Infrastructure.Persistence.Quizzes;
using Games.Infrastructure.Persistence.SocialChallengeExecutions;
using Games.Infrastructure.Persistence.SocialChallenges;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure;

namespace Games.Infrastructure;

public static class GamesModuleExtensions
{
    public static IServiceCollection AddGamesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<GamesDbContext>(configuration, "games");
        services.AddHostedService<GamesModuleInitializer>();
        services.AddAutoMapper(mapper => mapper.AddProfile<GamesMapperProfile>());

        services.AddScoped<IQuizRepository, QuizRepository>();
        services.AddScoped<ISocialChallengeRepository, SocialChallengeRepository>();
        services.AddScoped<ILocationChallengeRepository, LocationChallengeRepository>();
        services.AddScoped<ILocationChallengeParticipationRepository, LocationChallengeParticipationRepository>();
        services.AddScoped<IQuizParticipationRepository, QuizParticipationRepository>();
        services.AddScoped<ISocialChallengeExecutionRepository, SocialChallengeExecutionRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<GamesDbContext>());
        services.AddScoped<IQuizReadRepository, QuizReadRepository>();
        services.AddScoped<ISocialChallengeReadRepository, SocialChallengeReadRepository>();
        services.AddScoped<ILocationChallengeReadRepository, LocationChallengeReadRepository>();
        services.AddScoped<ILocationChallengeParticipationReadRepository, LocationChallengeParticipationReadRepository>();

        services.AddScoped<QuizAuthoringService>();
        services.AddScoped<QuizAuthoringQueries>();
        services.AddScoped<QuizLifecycleService>();
        services.AddScoped<SocialChallengeAuthoringService>();
        services.AddScoped<SocialChallengeAuthoringQueries>();
        services.AddScoped<SocialChallengeLifecycleService>();
        services.AddScoped<LocationChallengeAuthoringService>();
        services.AddScoped<LocationChallengeAuthoringQueries>();
        services.AddScoped<LocationChallengeHunt>();
        services.AddScoped<LocationChallengeSolvingService>();
        services.AddScoped<LocationChallengeSolvingQueries>();
        services.AddScoped<QuizPlay>();
        services.AddScoped<QuizPlayingService>();
        services.AddScoped<QuizPlayingQueries>();
        services.AddScoped<SocialChallengePlay>();
        services.AddScoped<SocialChallengePlayingService>();
        services.AddScoped<SocialChallengePlayingQueries>();

        return services;
    }
}
