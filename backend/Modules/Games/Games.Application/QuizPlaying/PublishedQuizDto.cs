using Games.Application.Quizzes;

namespace Games.Application.QuizPlaying;

// A published quiz as the asking explorer sees it: whether they solved it and how many of its
// questions they have answered correctly so far.
public sealed record PublishedQuizDto(
    Guid Id,
    string Title,
    string Description,
    int ExperiencePoints,
    LocationDto Location,
    int QuestionCount,
    bool IsSolved,
    int CorrectAnswerCount);
