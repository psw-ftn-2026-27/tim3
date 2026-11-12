using Games.Domain.Quizzes;
using Shared.Tests;

namespace Games.Tests.Integration.Seeds;

internal static class QuizSeed
{
    public static readonly Guid OtherAdministrator = Guid.Parse("00000000-0000-0000-0000-0000000000a2");

    public static readonly Quiz EmptyMuseumQuiz = new(WellKnownUsers.Administrator, "Muzej Vojvodine", "", 10);
    public static readonly Quiz FortressQuiz;
    public static readonly Quiz OtherAdministratorsQuiz;
    public static readonly Quiz ReadyToPublishCathedralQuiz;
    public static readonly Quiz PublishedSynagogueQuiz;
    public static readonly Quiz RetiredBridgeQuiz;

    static QuizSeed()
    {
        FortressQuiz = new(WellKnownUsers.Administrator, "Petrovaradinska tvrđava", "Pitanja o istoriji tvrđave.", 50);
        FortressQuiz.AddQuestion("Na kojoj reci je tvrđava?", [new Answer("Dunav", true), new Answer("Sava", false), new Answer("Tisa", false)]);

        OtherAdministratorsQuiz = new(OtherAdministrator, "Trg slobode", "Kviz drugog administratora.", 20);
        OtherAdministratorsQuiz.AddQuestion("Koji je trg glavni?", [new Answer("Trg slobode", true), new Answer("Trg mladenaca", false)]);

        ReadyToPublishCathedralQuiz = new(WellKnownUsers.Administrator, "Katedrala", "Odgovori na tri pitanja o katedrali.", 30);
        ReadyToPublishCathedralQuiz.AddQuestion("Na kom trgu je katedrala?", [new Answer("Trg slobode", true), new Answer("Trg galerija", false)]);
        ReadyToPublishCathedralQuiz.AddQuestion("Kom svecu je posvećena?", [new Answer("Svetoj Mariji", true), new Answer("Svetom Đorđu", false)]);
        ReadyToPublishCathedralQuiz.AddQuestion("U kom stilu je građena?", [new Answer("Neogotičkom", true), new Answer("Baroknom", false), new Answer("Romanskom", false)]);

        PublishedSynagogueQuiz = new(WellKnownUsers.Administrator, "Sinagoga", "Odgovori na tri pitanja o sinagogi.", 40);
        PublishedSynagogueQuiz.AddQuestion("U kojoj ulici je sinagoga?", [new Answer("Jevrejskoj", true), new Answer("Zmaj Jovinoj", false)]);
        PublishedSynagogueQuiz.AddQuestion("Ko je projektovao sinagogu?", [new Answer("Lipot Baumhorn", true), new Answer("Ödön Lechner", false)]);
        PublishedSynagogueQuiz.AddQuestion("Za šta se sinagoga danas koristi?", [new Answer("Koncerte", true), new Answer("Sport", false)]);
        PublishedSynagogueQuiz.Publish();

        RetiredBridgeQuiz = new(WellKnownUsers.Administrator, "Mostovi", "Odgovori na tri pitanja o mostovima.", 25);
        RetiredBridgeQuiz.AddQuestion("Koliko mostova ima Novi Sad?", [new Answer("Tri", true), new Answer("Sedam", false)]);
        RetiredBridgeQuiz.AddQuestion("Koji most je najstariji?", [new Answer("Varadinski", true), new Answer("Most slobode", false)]);
        RetiredBridgeQuiz.AddQuestion("Preko koje reke vode?", [new Answer("Dunav", true), new Answer("Tisa", false)]);
        RetiredBridgeQuiz.Publish();
        RetiredBridgeQuiz.Retire();
    }

    public static Quiz[] All => [EmptyMuseumQuiz, FortressQuiz, OtherAdministratorsQuiz, ReadyToPublishCathedralQuiz, PublishedSynagogueQuiz, RetiredBridgeQuiz];
}
