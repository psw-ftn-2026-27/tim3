namespace Identity.Core;

// The user name is the e-mail address; other modules see only the part before the @.
internal static class DisplayName
{
    public static string From(string userName)
    {
        var at = userName.IndexOf('@');
        return at < 0 ? userName : userName[..at];
    }
}
