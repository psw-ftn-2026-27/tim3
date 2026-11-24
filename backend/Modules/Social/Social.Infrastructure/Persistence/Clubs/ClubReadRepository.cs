using Microsoft.EntityFrameworkCore;
using Shared.Domain;
using Social.Application.ClubManagement;
using Social.Application.ClubMembership;
using Social.Application.ClubRoster;
using Social.Application.Clubs;
using Social.Domain.Clubs;

namespace Social.Infrastructure.Persistence.Clubs;

internal sealed class ClubReadRepository : IClubReadRepository
{
    private readonly SocialDbContext _dbContext;

    public ClubReadRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PageResult<ClubDto>> GetPageAsync(int page, int pageSize)
    {
        var clubs = _dbContext.Clubs
            .OrderBy(club => club.Name)
            .ThenBy(club => club.Id);

        var totalCount = await clubs.CountAsync();
        var items = await ProjectToDtos(clubs)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<ClubDto>(items, totalCount);
    }

    public Task<ClubDetailsRowDto?> GetDetailsAsync(Guid clubId)
    {
        return _dbContext.Clubs
            .Where(club => club.Id == clubId)
            .Select(club => new ClubDetailsRowDto(
                club.Id,
                club.OwnerId,
                club.Name,
                club.Description,
                club.Images,
                club.Members.Select(member => member.UserId).ToList(),
                club.Members
                    .Where(member => member.Role == ClubRole.Moderator)
                    .Select(member => member.UserId)
                    .ToList(),
                club.JoinRequests.Select(request => request.UserId).ToList()))
            .FirstOrDefaultAsync();
    }

    public Task<ClubInvitationsRowDto?> GetInvitationsAsync(Guid clubId)
    {
        return _dbContext.Clubs
            .Where(club => club.Id == clubId)
            .Select(club => new ClubInvitationsRowDto(
                club.OwnerId,
                club.Invitations.Select(invitation => invitation.UserId).ToList()))
            .FirstOrDefaultAsync();
    }

    public Task<List<ReceivedInvitationDto>> GetInvitationsForAsync(Guid userId)
    {
        return _dbContext.Clubs
            .Where(club => club.Invitations.Any(invitation => invitation.UserId == userId))
            .OrderBy(club => club.Name)
            .ThenBy(club => club.Id)
            .Select(club => new ReceivedInvitationDto(club.Id, club.Name))
            .ToListAsync();
    }

    private static IQueryable<ClubDto> ProjectToDtos(IQueryable<Club> clubs)
    {
        return clubs
            .Select(club => new ClubDto(
                club.Id,
                club.OwnerId,
                club.Name,
                club.Description,
                club.Images));
    }
}
