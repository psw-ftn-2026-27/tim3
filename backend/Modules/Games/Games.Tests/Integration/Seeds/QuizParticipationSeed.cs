using Games.Domain.QuizParticipations;
using Shared.Tests;

namespace Games.Tests.Integration.Seeds;

internal static class QuizParticipationSeed
{
    private static readonly DateTime AnsweredAt = new(2026, 11, 1, 10, 0, 0, DateTimeKind.Utc);

    public static readonly QuizParticipation SynagogueBeforeLastAnswerOfFirstPass;
    public static readonly QuizParticipation SolvedDanubeParkWithTwoCorrect;
    public static readonly QuizParticipation SynagogueSolvedBySecondExplorer;

    static QuizParticipationSeed()
    {
        SynagogueBeforeLastAnswerOfFirstPass = new(QuizSeed.PublishedSynagogueQuiz.Id, WellKnownUsers.Explorer);
        SynagogueBeforeLastAnswerOfFirstPass.StartPass(QuizSeed.PublishedSynagogueQuiz.Questions.Select(question => question.Id).ToList());
        SynagogueBeforeLastAnswerOfFirstPass.Record(QuizSeed.PublishedSynagogueQuiz.Questions[0].Id, 0, true, AnsweredAt);
        SynagogueBeforeLastAnswerOfFirstPass.Record(QuizSeed.PublishedSynagogueQuiz.Questions[1].Id, 1, false, AnsweredAt);

        SolvedDanubeParkWithTwoCorrect = new(QuizSeed.PublishedDanubeParkQuiz.Id, WellKnownUsers.Explorer);
        SolvedDanubeParkWithTwoCorrect.StartPass(QuizSeed.PublishedDanubeParkQuiz.Questions.Select(question => question.Id).ToList());
        SolvedDanubeParkWithTwoCorrect.Record(QuizSeed.PublishedDanubeParkQuiz.Questions[0].Id, 0, true, AnsweredAt);
        SolvedDanubeParkWithTwoCorrect.Record(QuizSeed.PublishedDanubeParkQuiz.Questions[1].Id, 0, true, AnsweredAt);
        SolvedDanubeParkWithTwoCorrect.Record(QuizSeed.PublishedDanubeParkQuiz.Questions[2].Id, 1, false, AnsweredAt);
        SolvedDanubeParkWithTwoCorrect.Solve(AnsweredAt, 40);

        SynagogueSolvedBySecondExplorer = new(QuizSeed.PublishedSynagogueQuiz.Id, WellKnownUsers.SecondExplorer);
        SynagogueSolvedBySecondExplorer.StartPass(QuizSeed.PublishedSynagogueQuiz.Questions.Select(question => question.Id).ToList());
        SynagogueSolvedBySecondExplorer.Record(QuizSeed.PublishedSynagogueQuiz.Questions[0].Id, 0, true, AnsweredAt);
        SynagogueSolvedBySecondExplorer.Record(QuizSeed.PublishedSynagogueQuiz.Questions[1].Id, 0, true, AnsweredAt);
        SynagogueSolvedBySecondExplorer.Record(QuizSeed.PublishedSynagogueQuiz.Questions[2].Id, 0, true, AnsweredAt);
        SynagogueSolvedBySecondExplorer.Solve(AnsweredAt, 40);
    }

    public static QuizParticipation[] All => [SynagogueBeforeLastAnswerOfFirstPass, SolvedDanubeParkWithTwoCorrect, SynagogueSolvedBySecondExplorer];
}
