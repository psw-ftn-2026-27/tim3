using Games.Application.QuizParticipations;
using Games.Application.Quizzes;
using Games.Domain.Locations;
using Games.Domain.QuizParticipations;
using Games.Domain.Quizzes;
using Maps.Contracts;
using Shared.Domain.Exceptions;

namespace Games.Application.QuizPlaying;

public sealed class QuizPlayingService
{
    private readonly IQuizRepository _quizRepository;
    private readonly IQuizParticipationRepository _participationRepository;
    private readonly IMapsApi _mapsApi;
    private readonly QuizPlay _quizPlay;
    private readonly IUnitOfWork _unitOfWork;

    public QuizPlayingService(
        IQuizRepository quizRepository,
        IQuizParticipationRepository participationRepository,
        IMapsApi mapsApi,
        QuizPlay quizPlay,
        IUnitOfWork unitOfWork)
    {
        _quizRepository = quizRepository;
        _participationRepository = participationRepository;
        _mapsApi = mapsApi;
        _quizPlay = quizPlay;
        _unitOfWork = unitOfWork;
    }

    public async Task ActivateAsync(Guid quizId, Guid explorerId)
    {
        var quiz = await GetQuizAsync(quizId);
        var position = await _mapsApi.GetCurrentPositionAsync(explorerId)
            ?? throw new DomainException("Unable to access your location.");
        var existingParticipation = await _participationRepository.GetAsync(quizId, explorerId);
        var participation = existingParticipation ?? new QuizParticipation(quizId, explorerId);

        _quizPlay.Activate(quiz, participation,
            new Location(position.Latitude, position.Longitude), position.SetAt, DateTime.UtcNow);
        if (existingParticipation is null)
        {
            _participationRepository.Add(participation);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SubmitAnswerAsync(Guid quizId, Guid explorerId, SubmitAnswerDto dto)
    {
        var quiz = await GetQuizAsync(quizId);
        var participation = await _participationRepository.GetAsync(quizId, explorerId)
            ?? throw new NotFoundException($"You have not started quiz {quizId}.");

        _quizPlay.Answer(quiz, participation, dto.QuestionId, dto.AnswerIndex, DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Quiz> GetQuizAsync(Guid quizId)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizId);
        if (quiz is null)
        {
            throw new NotFoundException("This quiz is not available.");
        }
        return quiz;
    }
}
