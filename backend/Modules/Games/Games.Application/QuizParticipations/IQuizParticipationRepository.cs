using Games.Domain.QuizParticipations;

namespace Games.Application.QuizParticipations;

public interface IQuizParticipationRepository
{
    Task<QuizParticipation?> GetAsync(Guid quizId, Guid explorerId);
    void Add(QuizParticipation participation);
}
