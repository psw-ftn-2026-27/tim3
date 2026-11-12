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
    }

    public static Quiz[] All => [EmptyMuseumQuiz, FortressQuiz, OtherAdministratorsQuiz, ReadyToPublishCathedralQuiz];
}
