import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PlayQuestionDto, QuizPlayDto } from '../../api/games-api-types';
import { commandFailure, QuizPlaying } from '../quiz-playing';

// The one question the page shows. A locked question was answered correctly in an earlier pass;
// an answered one was just answered in this pass and shows its result until the explorer clicks Next.
type ShownQuestion =
  | { kind: 'open'; question: PlayQuestionDto }
  | { kind: 'locked'; question: PlayQuestionDto; chosenAnswerIndex: number }
  | { kind: 'answered'; question: PlayQuestionDto; chosenAnswerIndex: number; correct: boolean };

@Component({
  imports: [RouterLink],
  selector: 'app-play-quiz',
  styleUrl: './play-quiz.scss',
  templateUrl: './play-quiz.html',
})
export class PlayQuiz {
  private readonly quizPlaying = inject(QuizPlaying);

  readonly id = input.required<string>();

  protected readonly play = httpResource<QuizPlayDto>(() => `/api/games/quizzes/${this.id()}/play`);

  protected readonly playErrorStatus = computed(() => {
    const failure = this.play.error();
    return failure instanceof HttpErrorResponse ? failure.status : null;
  });

  // The position the explorer has moved past with Next. Until the first Next, a new pass starts
  // from its first question and a resumed pass from its current question.
  private readonly reachedPosition = signal<number | null>(null);

  // The question just answered. The page keeps it, because the reloaded state already carries the next one.
  private readonly sent = signal<{ question: PlayQuestionDto; chosenAnswerIndex: number } | null>(null);

  protected readonly shown = computed<ShownQuestion | null>(() => {
    const state = this.play.value();
    if (!state) {
      return null;
    }
    const sent = this.sent();
    if (sent !== null) {
      // The result is known only once the state has been reloaded after the answer.
      return this.play.isLoading()
        ? { kind: 'open', question: sent.question }
        : { kind: 'answered', ...sent, correct: state.lastAnswerCorrect === true };
    }

    const current = state.currentQuestion;
    const startPosition = state.lastAnswerCorrect === null || current === null ? 1 : current.position;
    const from = this.reachedPosition() ?? startPosition;

    const locked = state.lockedQuestions
      .filter((question) => question.position >= from)
      .sort((first, second) => first.position - second.position)[0];
    if (current !== null && current.position >= from && (!locked || current.position < locked.position)) {
      return { kind: 'open', question: current };
    }
    if (locked) {
      return { kind: 'locked', question: locked, chosenAnswerIndex: locked.chosenAnswerIndex };
    }
    return null;
  });

  // The pass is over when the server has no next question and the page has nothing left to show.
  protected readonly finished = computed(
    () => this.play.value()?.currentQuestion === null && this.shown() === null,
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async answer(question: PlayQuestionDto, answerIndex: number): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.quizPlaying.submitAnswer(this.id(), { questionId: question.id, answerIndex });
      this.sent.set({ question, chosenAnswerIndex: answerIndex });
    } catch (error) {
      this.error.set(commandFailure(error));
    } finally {
      this.pending.set(false);
      this.play.reload();
    }
  }

  // Next is the only way to the following question, so the explorer reads each result first.
  protected next(): void {
    const shown = this.shown();
    if (shown === null) {
      return;
    }
    this.sent.set(null);
    this.reachedPosition.set(shown.question.position + 1);
  }
}
