namespace Games.Application.QuizPlaying;

// The state of the explorer's current pass. CurrentQuestion is null once the pass is finished;
// ExperienceGained is null until the first pass solves the quiz.
public sealed record QuizPlayDto(
    Guid QuizId,
    string Title,
    int Pass,
    int QuestionCount,
    PlayQuestionDto? CurrentQuestion,
    bool? LastAnswerCorrect,
    List<LockedQuestionDto> LockedQuestions,
    int CorrectAnswerCount,
    int? ExperienceGained);
