using AutoMapper;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Payment.Contracts;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourAuthoring;

public sealed class TourAuthoringService
{
    private readonly ITourRepository _tourRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPaymentApi _paymentApi;

    public TourAuthoringService(ITourRepository tourRepository, IUnitOfWork unitOfWork, IMapper mapper, IPaymentApi paymentApi)
    {
        _tourRepository = tourRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _paymentApi = paymentApi;
    }

    public async Task<TourDto> CreateAsync(Guid authorId, CreateTourDto dto)
    {
        var groupSize = dto.RecommendedGroupSize is null
            ? null
            : new GroupSize(dto.RecommendedGroupSize.From, dto.RecommendedGroupSize.To);
        var tour = new Tour(authorId, dto.Name, dto.Description, dto.Difficulty, dto.Tags, groupSize);

        _tourRepository.Add(tour);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<TourDto>(tour);
    }

    public async Task AddTransportTimeAsync(Guid tourId, Guid authorId, TransportTimeDto dto)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);

        tour.AddTransportTime(dto.Transport, dto.Minutes);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DefineKeyPointsAsync(Guid tourId, Guid authorId, DefineKeyPointsDto dto)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);

        var keyPoints = dto.KeyPoints.Select(ToKeyPoint).ToList();
        tour.DefineKeyPoints(keyPoints, new TourLength(dto.Length.Kilometres, dto.Length.IsStraightLine));
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task PublishAsync(Guid tourId, Guid authorId, PublishTourDto dto)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);

        var price = ToTourPrice(dto);
        tour.Publish(price);
        await _paymentApi.CreateProductAsync(tour.Id, tour.Name, tour.AuthorId, price?.Amount ?? 0);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ArchiveAsync(Guid tourId, Guid authorId)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);

        tour.Archive();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ReactivateAsync(Guid tourId, Guid authorId)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);

        tour.Reactivate();
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Tour> GetOwnedTourAsync(Guid tourId, Guid authorId)
    {
        var tour = await _tourRepository.GetByIdAsync(tourId);
        if (tour is null || tour.AuthorId != authorId)
        {
            throw new NotFoundException($"Tour {tourId} does not exist.");
        }
        return tour;
    }

    private static KeyPoint ToKeyPoint(KeyPointInputDto dto)
    {
        var location = dto.Location is null
            ? null
            : new GeoLocation(dto.Location.Latitude, dto.Location.Longitude);
        return new KeyPoint(dto.Name, dto.Description, dto.ImageAddress, location, dto.Secret, dto.VisibleWithoutPurchase);
    }

    private static TourPrice? ToTourPrice(PublishTourDto dto)
    {
        if (dto.IsFree is null)
        {
            return null;
        }
        return dto.IsFree.Value ? TourPrice.Free() : TourPrice.Paid(dto.Amount ?? 0);
    }
}
