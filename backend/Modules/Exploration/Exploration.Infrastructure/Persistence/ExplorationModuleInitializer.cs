using Exploration.Domain.Tours;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Exploration.Infrastructure.Persistence;

internal sealed class ExplorationModuleInitializer : IHostedService
{
    private static readonly Guid FirstExplorer = Guid.Parse("20000000-0000-0000-0000-000000000001");

    private readonly IServiceProvider _serviceProvider;
    private readonly IHostEnvironment _environment;

    public ExplorationModuleInitializer(IServiceProvider serviceProvider, IHostEnvironment environment)
    {
        _serviceProvider = serviceProvider;
        _environment = environment;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ExplorationDbContext>();
        await dbContext.Database.MigrateAsync();

        if (_environment.IsDevelopment() && !await dbContext.Tours.AnyAsync())
        {
            dbContext.Tours.AddRange(CreateDemoTours());
            await dbContext.SaveChangesAsync();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static List<Tour> CreateDemoTours()
    {
        var fortress = new Tour(FirstExplorer, "Šetnja Petrovaradinskom tvrđavom",
            "Šetnja počinje na Gornjem platou Petrovaradinske tvrđave, vodi pored Sahat kule i podzemnih vojnih galerija, a završava se pogledom na Dunav i stari deo Novog Sada.",
            TourDifficulty.Easy, ["istorija", "priroda"], new GroupSize(2, 10));
        fortress.AddTransportTime(TransportMode.Walking, 120);
        fortress.DefineKeyPoints(
        [
            new KeyPoint("Sahat kula", "Sat na kuli ima zamenjene kazaljke: mala pokazuje minute, a velika sate, da bi se videla sa reke.",
                "/api/media/images/demo-sahat-kula.jpg", new GeoLocation(45.2526, 19.8633), "Ispod kule počinje ulaz u podzemne galerije.", true),
            new KeyPoint("Vidikovac na Dunav", "Sa bedema se vide Most slobode, Štrand i stari deo Novog Sada.",
                "/api/media/images/demo-vidikovac.jpg", new GeoLocation(45.2540, 19.8615), null, false)
        ], new TourLength(0.6, false));
        fortress.Publish(TourPrice.Free());

        var fruskaGora = new Tour(FirstExplorer, "Biciklom preko Fruške gore",
            "Ruta vodi od Sremske Kamenice preko Popovice do Iriškog venca, pored manastira Beočin i vidikovaca sa kojih se vidi ceo Srem, uz nekoliko dužih uspona.",
            TourDifficulty.Hard, ["biciklizam", "planina"], new GroupSize(1, 4));
        fruskaGora.AddTransportTime(TransportMode.Bicycle, 240);
        fruskaGora.DefineKeyPoints(
        [
            new KeyPoint("Sremska Kamenica", "Polazna tačka rute, kod crkve u centru Sremske Kamenice.",
                "/api/media/images/demo-sremska-kamenica.jpg", new GeoLocation(45.2216, 19.8401), null, true),
            new KeyPoint("Iriški venac", "Najviša tačka rute, sa spomenikom i pogledom na ceo Srem.",
                "/api/media/images/demo-iriski-venac.jpg", new GeoLocation(45.1497, 19.8486), "Iza spomenika vodi staza do skrivenog izvora.", false)
        ], new TourLength(11.2, false));
        fruskaGora.Publish(TourPrice.Free());

        var cityCenter = new Tour(FirstExplorer, "Obilazak centra grada",
            "Kratak obilazak Trga slobode, Dunavske ulice i Dunavskog parka.",
            TourDifficulty.Moderate, ["grad"], new GroupSize(5, 5));

        return [fortress, fruskaGora, cityCenter];
    }
}
