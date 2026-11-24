using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.GiftCardPurchasing;
using Payment.Domain.GiftCards;
using Payment.Tests.Integration.Seeds;
using Shared.Domain;
using Xunit;

namespace Payment.Tests.Integration.GiftCardPurchasing;

public class GiftCardPurchasingQueryTests : BaseIntegrationTest
{
    public GiftCardPurchasingQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Buyer_sees_only_the_gift_cards_they_bought_with_the_recipients_name()
    {
        var buyerId = GiftCardSeed.DemoExplorerId;
        var expectedIds = GiftCardSeed.All.Where(card => card.BuyerId == buyerId).Select(card => card.Id).ToList();
        var expected = GiftCardSeed.DemoExplorerCardForSecondDemoExplorer;
        using var client = Factory.CreateClientFor(buyerId, "explorer");

        var cards = await client.GetFromJsonAsync<PageResult<PurchasedGiftCardDto>>(
            "/api/payment/gift-cards/purchased", JsonOptions);

        cards!.Items.Select(card => card.Id).Should().BeEquivalentTo(expectedIds);
        cards.TotalCount.Should().Be(expectedIds.Count);
        cards.Items.Should().ContainSingle(card => card.Id == expected.Id)
            .Which.Should().Be(new PurchasedGiftCardDto(
                expected.Id, expected.RecipientId, "explorer2", expected.Amount, expected.Message,
                expected.ExpiresAt, expected.StatusAt(DateTime.UtcNow)));
    }

    [Fact]
    public async Task Buyer_sees_a_gift_card_the_recipient_redeemed_as_redeemed()
    {
        var redeemed = GiftCardSeed.RedeemedCardOfGiftCardBuyer;
        using var client = Factory.CreateClientFor(redeemed.BuyerId, "explorer");

        var cards = await client.GetFromJsonAsync<PageResult<PurchasedGiftCardDto>>(
            "/api/payment/gift-cards/purchased?page=1&pageSize=100", JsonOptions);

        cards!.Items.Should().ContainSingle(card => card.Id == redeemed.Id)
            .Which.Status.Should().Be(GiftCardStatus.Redeemed);
    }

    [Fact]
    public async Task Buyer_pages_through_purchased_gift_cards_newest_first()
    {
        var buyerId = GiftCardSeed.DemoExplorerId;
        var bought = GiftCardSeed.All.Where(card => card.BuyerId == buyerId).ToList();
        var newest = bought.OrderByDescending(card => card.PurchasedAt).First();
        using var client = Factory.CreateClientFor(buyerId, "explorer");

        var cards = await client.GetFromJsonAsync<PageResult<PurchasedGiftCardDto>>(
            "/api/payment/gift-cards/purchased?page=1&pageSize=1", JsonOptions);

        cards!.Items.Should().ContainSingle().Which.Id.Should().Be(newest.Id);
        cards.TotalCount.Should().Be(bought.Count);
    }

    [Fact]
    public async Task Recipient_search_offers_other_explorers_but_not_the_buyer_or_administrators()
    {
        var buyerId = GiftCardSeed.DemoExplorerId;
        using var client = Factory.CreateClientFor(buyerId, "explorer");

        var recipients = await client.GetFromJsonAsync<PageResult<RecipientDto>>(
            "/api/payment/gift-cards/recipients?text=&page=1&pageSize=100", JsonOptions);

        var ids = recipients!.Items.Select(recipient => recipient.UserId).ToList();
        ids.Should().Contain([GiftCardSeed.SecondDemoExplorerId, GiftCardSeed.ThirdDemoExplorerId]);
        ids.Should().NotContain([buyerId, GiftCardSeed.DemoAdministratorId]);
    }

    [Fact]
    public async Task Recipient_search_matches_the_display_name()
    {
        using var client = Factory.CreateClientFor(GiftCardSeed.DemoExplorerId, "explorer");

        var recipients = await client.GetFromJsonAsync<PageResult<RecipientDto>>(
            "/api/payment/gift-cards/recipients?text=explorer2&page=1&pageSize=20", JsonOptions);

        recipients!.Items.Should().ContainSingle()
            .Which.Should().Be(new RecipientDto(GiftCardSeed.SecondDemoExplorerId, "explorer2"));
    }

    [Fact]
    public async Task Recipient_search_returns_the_requested_page_and_the_total_count()
    {
        using var client = Factory.CreateClientFor(GiftCardSeed.DemoExplorerId, "explorer");

        var secondPage = await client.GetFromJsonAsync<PageResult<RecipientDto>>(
            "/api/payment/gift-cards/recipients?text=explorer&page=2&pageSize=1", JsonOptions);

        secondPage!.Items.Should().ContainSingle()
            .Which.UserId.Should().Be(GiftCardSeed.ThirdDemoExplorerId);
        secondPage.TotalCount.Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task Recipient_search_far_past_the_last_page_returns_no_recipients()
    {
        using var client = Factory.CreateClientFor(GiftCardSeed.DemoExplorerId, "explorer");

        var response = await client.GetAsync(
            $"/api/payment/gift-cards/recipients?text=&page={int.MaxValue}&pageSize=100");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var recipients = await response.Content.ReadFromJsonAsync<PageResult<RecipientDto>>(JsonOptions);
        recipients!.Items.Should().BeEmpty();
    }
}
