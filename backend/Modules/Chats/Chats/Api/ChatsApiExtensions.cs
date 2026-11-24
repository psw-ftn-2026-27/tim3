using Microsoft.Extensions.DependencyInjection;

namespace Chats.Api;

public static class ChatsApiExtensions
{
    public static IMvcBuilder AddChatsControllers(this IMvcBuilder mvc)
    {
        return mvc.AddApplicationPart(typeof(ChatsApiExtensions).Assembly);
    }
}
