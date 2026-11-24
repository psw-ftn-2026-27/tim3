using Shared.Domain.Exceptions;
using Social.Application.Clubs;
using Social.Domain.Clubs;

namespace Social.Application.ClubManagement;

public sealed class ClubManagementService
{
    private readonly IClubRepository _clubRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClubManagementService(IClubRepository clubRepository, IUnitOfWork unitOfWork)
    {
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreateAsync(Guid userId, CreateClubDto dto)
    {
        var club = new Club(userId, dto.Name, dto.Description, dto.Images);
        var clubWithSameName = await _clubRepository.GetByNameIgnoringCaseAsync(club.Name);
        ClubNameUniqueness.EnsureUnique(club, clubWithSameName);

        _clubRepository.Add(club);
        await _unitOfWork.SaveChangesAsync();

        return club.Id;
    }

    public async Task UpdateAsync(Guid userId, Guid clubId, UpdateClubDto dto)
    {
        var club = await GetClubAsync(clubId);

        club.Update(userId, dto.Name, dto.Description, dto.Images);
        var clubWithSameName = await _clubRepository.GetByNameIgnoringCaseAsync(club.Name);
        ClubNameUniqueness.EnsureUnique(club, clubWithSameName);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid userId, Guid clubId)
    {
        var club = await GetClubAsync(clubId);

        club.EnsureCanBeDeletedBy(userId);
        _clubRepository.Remove(club);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Club> GetClubAsync(Guid clubId)
    {
        var club = await _clubRepository.GetByIdAsync(clubId);
        if (club is null)
        {
            throw new NotFoundException($"Club {clubId} does not exist.");
        }
        return club;
    }
}
