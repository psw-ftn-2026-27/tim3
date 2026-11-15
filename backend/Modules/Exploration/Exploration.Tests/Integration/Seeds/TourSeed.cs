using Exploration.Domain.Tours;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class TourSeed
{
    public static readonly Tour FortressWalk = new(WellKnownUsers.Explorer, "Šetnja tvrđavom", "Šetnja počinje na Gornjem platou Petrovaradinske tvrđave, vodi pored Sahat kule i podzemnih vojnih galerija, a završava se pogledom na Dunav.", TourDifficulty.Easy, ["istorija", "priroda"], new GroupSize(2, 10));
    public static readonly Tour CityStroll = new(WellKnownUsers.Explorer, "Šetnja centrom", "Kratak opis gradske šetnje.", TourDifficulty.Moderate, ["grad"], new GroupSize(4, 4));
    public static readonly Tour SecondExplorersTrail = new(WellKnownUsers.SecondExplorer, "Tuđa staza", "Kratak opis staze drugog autora.", TourDifficulty.Easy, ["grad"], new GroupSize(1, 3));
    public static readonly Tour PublishableRiverside;
    public static readonly Tour PublishedVineyards;
    public static readonly Tour PublishedMonasteries;
    public static readonly Tour ArchivedHomesteads;

    static TourSeed()
    {
        PublishableRiverside = new(WellKnownUsers.Explorer, "Staza uz Dunav", "Staza kreće od Ribarskog ostrva, prati obalu Dunava pored gradske plaže Štrand i završava se kod Mosta slobode, uz više mesta za predah.", TourDifficulty.Easy, ["priroda"], new GroupSize(1, 6));
        PublishableRiverside.AddTransportTime(TransportMode.Walking, 90);

        PublishedVineyards = new(WellKnownUsers.Explorer, "Vinogradi Sremskih Karlovaca", "Tura vodi kroz vinograde na obroncima Fruške gore, uz obilazak dva porodična podruma i degustaciju bermeta u Sremskim Karlovcima.", TourDifficulty.Moderate, ["vino", "priroda"], new GroupSize(2, 8));
        PublishedVineyards.AddTransportTime(TransportMode.Bicycle, 60);
        PublishedVineyards.Publish(TourPrice.Free());

        PublishedMonasteries = new(WellKnownUsers.Explorer, "Fruškogorski manastiri", "Tura obilazi tri fruškogorska manastira: Krušedol, Grgeteg i Novo Hopovo, sa pauzom za ručak u podnožju nacionalnog parka.", TourDifficulty.Hard, ["istorija"], new GroupSize(3, 12));
        PublishedMonasteries.AddTransportTime(TransportMode.Car, 45);
        PublishedMonasteries.Publish(TourPrice.Free());

        ArchivedHomesteads = new(WellKnownUsers.Explorer, "Bački salaši", "Tura obilazi tri salaša u okolini Novog Sada, uz upoznavanje sa starim zanatima, domaćom kuhinjom i životom na ravnici nekada i danas.", TourDifficulty.Easy, ["tradicija", "hrana"], new GroupSize(5, 20));
        ArchivedHomesteads.AddTransportTime(TransportMode.Car, 30);
        ArchivedHomesteads.Publish(TourPrice.Free());
        ArchivedHomesteads.Archive();
    }

    public static Tour[] All => [FortressWalk, CityStroll, SecondExplorersTrail, PublishableRiverside, PublishedVineyards, PublishedMonasteries, ArchivedHomesteads];
}
