using Exploration.Application.Tours;
using Payment.Contracts;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourBrowsing;

public sealed class TourBrowsingQueries
{
    private readonly ITourReadRepository _tourReadRepository;
    private readonly IPaymentApi _paymentApi;

    public TourBrowsingQueries(ITourReadRepository tourReadRepository, IPaymentApi paymentApi)
    {
        _tourReadRepository = tourReadRepository;
        _paymentApi = paymentApi;
    }

    public async Task<PageResult<PublishedTourDto>> GetPublishedAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var published = await _tourReadRepository.GetPublishedAsync(page, pageSize);
        var prices = await _paymentApi.GetPricesByTourIdsAsync(published.Items.Select(tour => tour.Id).ToList());
        var items = published.Items.Select(tour => new PublishedTourDto(
            tour.Id, tour.AuthorId, tour.Name, tour.Description, tour.Difficulty, tour.Tags, tour.Status,
            tour.PublishedAt, tour.ArchivedAt, tour.RecommendedGroupSize, tour.TransportTimes,
            prices.TryGetValue(tour.Id, out var price) ? price : null)).ToList();
        return new PageResult<PublishedTourDto>(items, published.TotalCount);
    }

    public async Task<TourDetailsDto> GetDetailsAsync(Guid tourId)
    {
        var details = await _tourReadRepository.GetPublishedDetailsAsync(tourId)
            ?? throw new NotFoundException($"Tour {tourId} does not exist.");
        var prices = await _paymentApi.GetPricesByTourIdsAsync([tourId]);
        return details with { PriceInEc = prices.TryGetValue(tourId, out var price) ? price : null };
    }

    public async Task<PageResult<TourDto>> GetMyPurchasedToursAsync(Guid userId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var tourIds = await _paymentApi.GetPurchasedTourIdsAsync(userId);
        return await _tourReadRepository.GetByIdsAsync(tourIds, page, pageSize);
    }

    public async Task<TourDto> GetPurchasedTourDetailsAsync(Guid userId, Guid tourId)
    {
        if (!await _paymentApi.OwnsTourAsync(userId, tourId))
        {
            throw new NotFoundException("Purchased tour is unavailable.");
        }

        return await _tourReadRepository.GetByIdAsync(tourId)
            ?? throw new NotFoundException("Purchased tour is unavailable.");
    }
}
