using Exploration.Application.TourPreferences;
using Exploration.Domain.TourPreferences;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourPreferenceManagement;

public sealed class TourPreferenceManagementService
{
    private readonly ITourPreferenceRepository _preferenceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TourPreferenceManagementService(ITourPreferenceRepository preferenceRepository, IUnitOfWork unitOfWork)
    {
        _preferenceRepository = preferenceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task SaveAsync(Guid explorerId, SaveTourPreferenceDto dto)
    {
        var preference = await _preferenceRepository.GetByExplorerIdAsync(explorerId);
        if (preference is null)
        {
            preference = new TourPreference(explorerId, dto.Difficulties, dto.Transport, dto.Tags, dto.AppliesToSearch);
            _preferenceRepository.Add(preference);
        }
        else
        {
            preference.Change(dto.Difficulties, dto.Transport, dto.Tags, dto.AppliesToSearch);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid explorerId)
    {
        var preference = await _preferenceRepository.GetByExplorerIdAsync(explorerId);
        if (preference is null)
        {
            throw new NotFoundException("The explorer has no tour preference.");
        }

        _preferenceRepository.Remove(preference);
        await _unitOfWork.SaveChangesAsync();
    }
}
