using Exploration.Domain.Tours;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class TourSeed
{
    public static readonly Tour FortressWalk = new(WellKnownUsers.Explorer, "Šetnja tvrđavom", "Šetnja počinje na Gornjem platou Petrovaradinske tvrđave, vodi pored Sahat kule i podzemnih vojnih galerija, a završava se pogledom na Dunav.", TourDifficulty.Easy, ["istorija", "priroda"], new GroupSize(2, 10));
    public static readonly Tour CityStroll = new(WellKnownUsers.Explorer, "Šetnja centrom", "Kratak opis gradske šetnje.", TourDifficulty.Moderate, ["grad"], new GroupSize(4, 4));
    public static readonly Tour SecondExplorersTrail = new(WellKnownUsers.SecondExplorer, "Tuđa staza", "Kratak opis staze drugog autora.", TourDifficulty.Easy, ["grad"], new GroupSize(1, 3));
    public static readonly Tour PublishableRiverside;
    public static readonly Tour PublicationWithSavedOffer;
    public static readonly Tour PublishedVineyards;
    public static readonly Tour PublishedMonasteries;
    public static readonly Tour ArchivedHomesteads;
    public static readonly Tour PublishedOldTownWithHiddenMiddleKeyPoint;

    static TourSeed()
    {
        PublishableRiverside = new(WellKnownUsers.Explorer, "Staza uz Dunav", "Staza kreće od Ribarskog ostrva, prati obalu Dunava pored gradske plaže Štrand i završava se kod Mosta slobode, uz više mesta za predah.", TourDifficulty.Easy, ["priroda"], new GroupSize(1, 6));
        PublishableRiverside.AddTransportTime(TransportMode.Walking, 90);
        PublishableRiverside.DefineKeyPoints(
        [
            new KeyPoint("Ribarsko ostrvo", "Polazak sa šetališta na Ribarskom ostrvu.", "/api/media/images/ribarsko-ostrvo.png", new GeoLocation(45.2380, 19.8780), "Na kraju mola ribari ostavljaju mamac za goste.", true),
            new KeyPoint("Most slobode", "Kraj staze, ispod Mosta slobode.", "/api/media/images/most-slobode.png", new GeoLocation(45.2410, 19.8560), "Ispod drugog stuba mosta je stari natpis graditelja.", false)
        ], new TourLength(2.4, false));

        PublicationWithSavedOffer = new(WellKnownUsers.Explorer, "Staza kroz Kamenicu", "Obilazak počinje u centru Sremske Kamenice, prolazi kraj Zmaj Jovine kuće i vodi do Kameničkog parka, gde se završava pogledom na obalu Dunava.", TourDifficulty.Easy, ["priroda"], new GroupSize(2, 8));
        PublicationWithSavedOffer.AddTransportTime(TransportMode.Walking, 70);
        PublicationWithSavedOffer.DefineKeyPoints(PublishableRiverside.KeyPoints.Select(point => new KeyPoint(point.Name, point.Description, point.ImageAddress, point.Location, point.Secret, point.VisibleWithoutPurchase)).ToList(), PublishableRiverside.Length);

        PublishedVineyards = new(WellKnownUsers.Explorer, "Vinogradi Sremskih Karlovaca", "Tura vodi kroz vinograde na obroncima Fruške gore, uz obilazak dva porodična podruma i degustaciju bermeta u Sremskim Karlovcima.", TourDifficulty.Moderate, ["vino", "priroda"], new GroupSize(2, 8));
        PublishedVineyards.AddTransportTime(TransportMode.Bicycle, 60);
        PublishedVineyards.DefineKeyPoints(
        [
            new KeyPoint("Porodični podrum", "Prvi podrum na ruti, sa degustacijom bermeta.", "/api/media/images/podrum.png", new GeoLocation(45.2030, 19.9330), "Domaćin čuva bocu iz godine osnivanja podruma.", true),
            new KeyPoint("Vinograd na obronku", "Vinograd sa pogledom na Dunav.", "/api/media/images/vinograd.png", new GeoLocation(45.1950, 19.9250), "Iza poslednjeg reda loze je stara vinogradarska kućica.", false)
        ], new TourLength(3.1, true));
        PublishedVineyards.Publish(TourPrice.Free());

        PublishedMonasteries = new(WellKnownUsers.Explorer, "Fruškogorski manastiri", "Tura obilazi tri fruškogorska manastira: Krušedol, Grgeteg i Novo Hopovo, sa pauzom za ručak u podnožju nacionalnog parka.", TourDifficulty.Hard, ["istorija"], new GroupSize(3, 12));
        PublishedMonasteries.AddTransportTime(TransportMode.Car, 45);
        PublishedMonasteries.DefineKeyPoints(
        [
            new KeyPoint("Krušedol", "Manastir Krušedol, zadužbina porodice Branković.", "/api/media/images/krusedol.png", new GeoLocation(45.1190, 19.9420), null, true),
            new KeyPoint("Grgeteg", "Manastir Grgeteg na južnim padinama Fruške gore.", "/api/media/images/grgeteg.png", new GeoLocation(45.1430, 19.8930), null, false)
        ], new TourLength(14.7, false));
        PublishedMonasteries.Publish(TourPrice.Free());

        ArchivedHomesteads = new(WellKnownUsers.Explorer, "Bački salaši", "Tura obilazi tri salaša u okolini Novog Sada, uz upoznavanje sa starim zanatima, domaćom kuhinjom i životom na ravnici nekada i danas.", TourDifficulty.Easy, ["tradicija", "hrana"], new GroupSize(5, 20));
        ArchivedHomesteads.AddTransportTime(TransportMode.Car, 30);
        ArchivedHomesteads.DefineKeyPoints(
        [
            new KeyPoint("Prvi salaš", "Salaš sa radionicom starih zanata.", "/api/media/images/prvi-salas.png", new GeoLocation(45.3050, 19.8100), null, false),
            new KeyPoint("Drugi salaš", "Salaš sa domaćom kuhinjom.", "/api/media/images/drugi-salas.png", new GeoLocation(45.3200, 19.7900), "U ambaru je skriven stari razboj.", false)
        ], new TourLength(6.5, false));
        ArchivedHomesteads.Publish(TourPrice.Free());
        ArchivedHomesteads.Archive();

        PublishedOldTownWithHiddenMiddleKeyPoint = new(WellKnownUsers.Explorer, "Stari grad Novog Sada", "Tura polazi sa Trga slobode, vodi kroz Zmaj Jovinu i Dunavsku ulicu do Dunavskog parka i otkriva priče o građanima koji su obeležili stari Novi Sad.", TourDifficulty.Easy, ["istorija", "grad"], new GroupSize(2, 15));
        PublishedOldTownWithHiddenMiddleKeyPoint.AddTransportTime(TransportMode.Walking, 75);
        PublishedOldTownWithHiddenMiddleKeyPoint.DefineKeyPoints(
        [
            new KeyPoint("Trg slobode", "Polazak ispred Gradske kuće.", "/api/media/images/trg-slobode.png", new GeoLocation(45.2551, 19.8451), "Na fasadi Gradske kuće jedna figura gleda na suprotnu stranu od ostalih.", true),
            new KeyPoint("Zmaj Jovina ulica", "Pešačka ulica sa kućom Zmaj Jove.", "/api/media/images/zmaj-jovina.png", new GeoLocation(45.2557, 19.8470), "U pasažu je sačuvan bunar iz osamnaestog veka.", false),
            new KeyPoint("Dunavski park", "Kraj ture kod jezerceta u parku.", "/api/media/images/dunavski-park.png", new GeoLocation(45.2545, 19.8530), "Najstarije drvo u parku posadio je prvi upravnik parka.", true)
        ], new TourLength(1.3, true));
        PublishedOldTownWithHiddenMiddleKeyPoint.Publish(TourPrice.Free());
    }

    public static Tour[] All => [FortressWalk, CityStroll, SecondExplorersTrail, PublishableRiverside, PublicationWithSavedOffer, PublishedVineyards, PublishedMonasteries, ArchivedHomesteads, PublishedOldTownWithHiddenMiddleKeyPoint];
}
