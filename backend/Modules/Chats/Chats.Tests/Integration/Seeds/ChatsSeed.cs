namespace Chats.Tests.Integration.Seeds;

internal static class ChatsSeed
{
    public static object[] All => [.. ChatSeed.All, .. MessageSeed.All];
}
