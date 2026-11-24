using Chats.Core;
using Chats.Core.ChatMessaging;
using Chats.Core.ChatModeration;
using Chats.Core.Chats;
using Chats.Core.Messages;
using Chats.Infrastructure.Persistence;
using Chats.Infrastructure.Persistence.Chats;
using Chats.Infrastructure.Persistence.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure;

namespace Chats.Infrastructure;

public static class ChatsModuleExtensions
{
    public static IServiceCollection AddChatsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<ChatsDbContext>(configuration, "chats");
        services.AddHostedService<ChatsModuleInitializer>();

        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<IChatReadRepository, ChatReadRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IMessageReadRepository, MessageReadRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ChatsDbContext>());
        services.AddScoped<ChatMessagingService>();
        services.AddScoped<ChatMessagingQueries>();
        services.AddScoped<ChatPrivilegesLookup>();
        services.AddScoped<ChatModerationService>();

        return services;
    }
}
