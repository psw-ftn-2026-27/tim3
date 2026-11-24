using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Unit.Tours;

public class TourTests
{
    private static readonly string LongDescription = new('o', 100);

    private static Tour CreateTour(string description) =>
        new(WellKnownUsers.Explorer, "Šetnja tvrđavom", description, TourDifficulty.Easy, ["istorija"], new GroupSize(2, 6));

    private static KeyPoint CreateKeyPoint(string name) =>
        new(name, "Opis ključne tačke.", "/api/media/images/tacka.png", new GeoLocation(45.25, 19.86), "Tajna tačke.", false);

    private static void DefineTwoKeyPoints(Tour tour) =>
        tour.DefineKeyPoints([CreateKeyPoint("Sahat kula"), CreateKeyPoint("Gornji plato")], new TourLength(1.2, false));

    private static Tour CreatePublishedTour()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Walking, 120);
        DefineTwoKeyPoints(tour);
        tour.Publish(TourPrice.Free());
        return tour;
    }

    private static Tour CreateArchivedTour()
    {
        var tour = CreatePublishedTour();
        tour.Archive();
        return tour;
    }

    [Fact]
    public void New_tour_starts_as_a_draft()
    {
        var tour = CreateTour(LongDescription);

        tour.Status.Should().Be(TourStatus.Draft);
        tour.PublishedAt.Should().BeNull();
        tour.TransportTimes.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_tour_requires_a_name(string name)
    {
        var creation = () => new Tour(WellKnownUsers.Explorer, name, "Opis ture.", TourDifficulty.Easy, ["istorija"], new GroupSize(2, 6));

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_tour_requires_a_description(string description)
    {
        var creation = () => CreateTour(description);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void New_tour_requires_tags()
    {
        var creation = () => new Tour(WellKnownUsers.Explorer, "Šetnja tvrđavom", "Opis ture.", TourDifficulty.Easy, [], new GroupSize(2, 6));

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void New_tour_requires_a_recommended_group_size()
    {
        var creation = () => new Tour(WellKnownUsers.Explorer, "Šetnja tvrđavom", "Opis ture.", TourDifficulty.Easy, ["istorija"], null);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_has_one_time_per_transport_mode()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Walking, 120);

        var addition = () => tour.AddTransportTime(TransportMode.Walking, 90);

        addition.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-30)]
    public void Transport_time_must_be_positive(int minutes)
    {
        var tour = CreateTour(LongDescription);

        var addition = () => tour.AddTransportTime(TransportMode.Walking, minutes);

        addition.Should().Throw<DomainException>();
    }

    [Fact]
    public void Free_tour_is_published_when_all_rules_are_met()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Bicycle, 45);
        DefineTwoKeyPoints(tour);

        tour.Publish(TourPrice.Free());

        tour.Status.Should().Be(TourStatus.Published);
        tour.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public void Paid_tour_is_published_when_all_rules_are_met()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Bicycle, 45);
        DefineTwoKeyPoints(tour);

        tour.Publish(TourPrice.Paid(1500));

        tour.Status.Should().Be(TourStatus.Published);
        tour.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public void Tour_cannot_be_published_without_a_price()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Walking, 120);
        DefineTwoKeyPoints(tour);

        var publishing = () => tour.Publish(null);

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_with_a_short_description_cannot_be_published()
    {
        var tour = CreateTour("Kratak opis.");
        tour.AddTransportTime(TransportMode.Walking, 120);
        DefineTwoKeyPoints(tour);

        var publishing = () => tour.Publish(TourPrice.Free());

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_without_a_transport_time_cannot_be_published()
    {
        var tour = CreateTour(LongDescription);
        DefineTwoKeyPoints(tour);

        var publishing = () => tour.Publish(TourPrice.Free());

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Published_tour_cannot_be_published_again()
    {
        var tour = CreatePublishedTour();

        var publishing = () => tour.Publish(TourPrice.Free());

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Published_tour_is_archived_with_its_archiving_time()
    {
        var tour = CreatePublishedTour();

        tour.Archive();

        tour.Status.Should().Be(TourStatus.Archived);
        tour.ArchivedAt.Should().NotBeNull();
    }

    [Fact]
    public void Draft_tour_cannot_be_archived()
    {
        var tour = CreateTour(LongDescription);

        var archiving = () => tour.Archive();

        archiving.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_tour_cannot_be_archived_again()
    {
        var tour = CreateArchivedTour();

        var archiving = () => tour.Archive();

        archiving.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_tour_is_reactivated_keeping_its_publishing_time()
    {
        var tour = CreateArchivedTour();
        var publishedAt = tour.PublishedAt;

        tour.Reactivate();

        tour.Status.Should().Be(TourStatus.Published);
        tour.PublishedAt.Should().Be(publishedAt);
        tour.ArchivedAt.Should().BeNull();
    }

    [Fact]
    public void Draft_tour_cannot_be_reactivated()
    {
        var tour = CreateTour(LongDescription);

        var reactivation = () => tour.Reactivate();

        reactivation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Published_tour_cannot_be_reactivated()
    {
        var tour = CreatePublishedTour();

        var reactivation = () => tour.Reactivate();

        reactivation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_tour_cannot_be_published_again()
    {
        var tour = CreateArchivedTour();

        var publishing = () => tour.Publish(TourPrice.Free());

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_tour_does_not_accept_a_transport_time()
    {
        var tour = CreateArchivedTour();

        var addition = () => tour.AddTransportTime(TransportMode.Bicycle, 45);

        addition.Should().Throw<DomainException>();
    }

    [Fact]
    public void Defined_key_points_replace_the_existing_ones_in_the_given_order_with_the_given_length()
    {
        var tour = CreateTour(LongDescription);
        DefineTwoKeyPoints(tour);
        List<KeyPoint> keyPoints = [CreateKeyPoint("Ulaz"), CreateKeyPoint("Galerije"), CreateKeyPoint("Vidikovac")];

        tour.DefineKeyPoints(keyPoints, new TourLength(2.5, false));

        tour.KeyPoints.Should().Equal(keyPoints);
        tour.KeyPoints.Select(keyPoint => keyPoint.Order).Should().Equal(1, 2, 3);
        tour.Length.Should().Be(new TourLength(2.5, false));
    }

    [Theory]
    [InlineData(TourStatus.Published)]
    [InlineData(TourStatus.Archived)]
    public void Tour_that_is_not_a_draft_does_not_accept_key_points(TourStatus status)
    {
        var tour = status == TourStatus.Published ? CreatePublishedTour() : CreateArchivedTour();
        var keyPointsBefore = tour.KeyPoints.ToList();

        var definition = () => tour.DefineKeyPoints([CreateKeyPoint("Ulaz"), CreateKeyPoint("Vidikovac")], new TourLength(1.5, false));

        definition.Should().Throw<DomainException>();
        tour.KeyPoints.Should().Equal(keyPointsBefore);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Draft_tour_accepts_fewer_than_two_key_points(int count)
    {
        var tour = CreateTour(LongDescription);
        DefineTwoKeyPoints(tour);
        var keyPoints = Enumerable.Range(1, count).Select(number => CreateKeyPoint($"Tačka {number}")).ToList();

        tour.DefineKeyPoints(keyPoints, new TourLength(0, false));

        tour.KeyPoints.Should().Equal(keyPoints);
    }

    [Fact]
    public void Tour_with_fewer_than_two_key_points_cannot_have_a_length_above_zero()
    {
        var tour = CreateTour(LongDescription);

        var definition = () => tour.DefineKeyPoints([CreateKeyPoint("Sahat kula")], new TourLength(0.8, false));

        definition.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_with_fewer_than_two_key_points_cannot_be_published()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Walking, 120);
        tour.DefineKeyPoints([CreateKeyPoint("Sahat kula")], new TourLength(0, false));

        var publishing = () => tour.Publish(TourPrice.Free());

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_with_a_straight_line_length_is_published()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Walking, 120);
        tour.DefineKeyPoints([CreateKeyPoint("Sahat kula"), CreateKeyPoint("Gornji plato")], new TourLength(0.9, true));

        tour.Publish(TourPrice.Free());

        tour.Status.Should().Be(TourStatus.Published);
    }
}
