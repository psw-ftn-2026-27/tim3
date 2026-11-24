using Shared.Domain.Exceptions;

namespace Social.Domain.Clubs;

public static class ClubNameUniqueness
{
    public static void EnsureUnique(Club club, Club? clubWithSameName)
    {
        if (clubWithSameName is null || clubWithSameName.Id == club.Id)
        {
            return;
        }
        if (string.Equals(club.Name, clubWithSameName.Name, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException($"A club named {club.Name} already exists.");
        }
    }
}
