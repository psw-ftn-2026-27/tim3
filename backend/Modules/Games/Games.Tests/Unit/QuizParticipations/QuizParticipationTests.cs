using FluentAssertions;
using Games.Domain.QuizParticipations;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Unit.QuizParticipations;

public class QuizParticipationTests
{
    private static readonly DateTime AnsweredAt = new(2026, 11, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly List<Guid> QuestionIds = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];

    private static QuizParticipation CreateStartedParticipation()
    {
        var participation = new QuizParticipation(Guid.NewGuid(), WellKnownUsers.Explorer);
        participation.StartPass(QuestionIds);
        return participation;
    }

    [Fact]
    public void Answer_to_a_question_that_is_not_next_in_the_pass_is_rejected()
    {
        var participation = CreateStartedParticipation();

        var recording = () => participation.Record(QuestionIds[1], 0, true, AnsweredAt);

        recording.Should().Throw<DomainException>();
        participation.Answers.Should().BeEmpty();
    }

    [Fact]
    public void Repeat_pass_rejects_an_answer_to_a_correctly_answered_question()
    {
        var participation = CreateStartedParticipation();
        participation.Record(QuestionIds[0], 0, true, AnsweredAt);
        participation.Record(QuestionIds[1], 1, false, AnsweredAt);
        participation.Record(QuestionIds[2], 1, false, AnsweredAt);
        participation.StartPass(QuestionIds);

        var recording = () => participation.Record(QuestionIds[0], 1, false, AnsweredAt);

        recording.Should().Throw<DomainException>();
        participation.Answers.Should().HaveCount(3);
    }

    [Fact]
    public void Participation_with_every_question_answered_correctly_cannot_start_another_pass()
    {
        var participation = CreateStartedParticipation();
        participation.Record(QuestionIds[0], 0, true, AnsweredAt);
        participation.Record(QuestionIds[1], 0, true, AnsweredAt);
        participation.Record(QuestionIds[2], 0, true, AnsweredAt);

        var start = () => participation.StartPass(QuestionIds);

        start.Should().Throw<DomainException>();
        participation.CurrentPass.Should().Be(1);
    }

    [Fact]
    public void Starting_again_in_the_middle_of_a_pass_resumes_from_the_first_unanswered_question()
    {
        var participation = CreateStartedParticipation();
        participation.Record(QuestionIds[0], 1, false, AnsweredAt);

        participation.StartPass(QuestionIds);

        participation.CurrentPass.Should().Be(1);
        participation.NextQuestionId().Should().Be(QuestionIds[1]);
    }
}
