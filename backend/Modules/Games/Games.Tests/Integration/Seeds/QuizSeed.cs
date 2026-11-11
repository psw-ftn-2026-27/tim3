using Games.Domain.Quizzes;
using Shared.Tests;

namespace Games.Tests.Integration.Seeds;

internal static class QuizSeed
{
    public static readonly Quiz EmptyMuseumQuiz = new(WellKnownUsers.Explorer, "Muzej Vojvodine", "", 10);
    public static readonly Quiz FortressQuiz;
    public static readonly Quiz SecondExplorersQuiz;

    static QuizSeed()
    {
        FortressQuiz = new(WellKnownUsers.Explorer, "Petrovaradinska tvrđava", "Pitanja o istoriji tvrđave.", 50);
        FortressQuiz.AddQuestion("Na kojoj reci je tvrđava?", [new Answer("Dunav", true), new Answer("Sava", false), new Answer("Tisa", false)]);

        SecondExplorersQuiz = new(WellKnownUsers.SecondExplorer, "Tuđi kviz", "Kviz drugog autora.", 20);
        SecondExplorersQuiz.AddQuestion("Koji je trg glavni?", [new Answer("Trg slobode", true), new Answer("Trg mladenaca", false)]);
    }

    public static Quiz[] All => [EmptyMuseumQuiz, FortressQuiz, SecondExplorersQuiz];
}
