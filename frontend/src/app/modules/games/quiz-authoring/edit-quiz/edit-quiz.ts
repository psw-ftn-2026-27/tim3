import { httpResource } from '@angular/common/http';
import { Component, effect, inject, input, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { serverMessage } from '../../../../shared/util/server-message';
import { AnswerDto, QuestionDto, QuizDto } from '../../api/games-api-types';
import { QuizAuthoring } from '../quiz-authoring';

const MAX_ANSWERS = 4;

@Component({
  imports: [FormField],
  selector: 'app-edit-quiz',
  styleUrl: './edit-quiz.scss',
  templateUrl: './edit-quiz.html',
})
export class EditQuiz {
  private readonly quizAuthoring = inject(QuizAuthoring);

  readonly id = input.required<string>();

  protected readonly quiz = httpResource<QuizDto>(() => `/api/games/quizzes/${this.id()}/edit`);

  protected readonly detailsForm = form(
    signal({
      title: '',
      description: '',
      latitude: null as number | null,
      longitude: null as number | null,
    }),
    (path) => {
      required(path.title, { message: 'Title is required.' });
    },
  );

  protected readonly questionForm = form(signal(this.questionModel(null)));

  // The question being edited, or null while the form adds a new question.
  protected readonly selectedQuestionId = signal<string | null>(null);

  protected readonly pending = signal(false);
  protected readonly detailsError = signal<string | null>(null);
  protected readonly questionError = signal<string | null>(null);

  private prefilled = false;

  constructor() {
    effect(() => {
      // Seed the details form once the author's quiz arrives from the server.
      const quiz = this.quiz.value();
      if (quiz && !this.prefilled) {
        this.prefilled = true;
        this.detailsForm().reset({
          title: quiz.title,
          description: quiz.description,
          latitude: quiz.location?.latitude ?? null,
          longitude: quiz.location?.longitude ?? null,
        });
      }
    });
  }

  protected async saveDetails(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.detailsError.set(null);
    const latitude = this.detailsForm.latitude().value();
    const longitude = this.detailsForm.longitude().value();
    try {
      await this.quizAuthoring.updateDetails(this.id(), {
        title: this.detailsForm.title().value(),
        description: this.detailsForm.description().value(),
        location:
          latitude !== null && longitude !== null ? { latitude, longitude } : null,
      });
      this.quiz.reload();
    } catch (failure) {
      this.detailsError.set(serverMessage(failure, 'Could not save the quiz.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected selectQuestion(question: QuestionDto): void {
    this.questionError.set(null);
    this.selectedQuestionId.set(question.id);
    this.questionForm().reset(this.questionModel(question));
  }

  protected cancelQuestionEdit(): void {
    this.questionError.set(null);
    this.selectedQuestionId.set(null);
    this.questionForm().reset(this.questionModel(null));
  }

  protected async saveQuestion(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.questionError.set(null);
    const dto = {
      text: this.questionForm.text().value(),
      // An answer field left empty and unmarked is not one of the offered answers.
      answers: this.questionForm
        .answers()
        .value()
        .filter((answer) => answer.text !== '' || answer.isCorrect),
    };
    try {
      const questionId = this.selectedQuestionId();
      if (questionId === null) {
        await this.quizAuthoring.addQuestion(this.id(), dto);
      } else {
        await this.quizAuthoring.updateQuestion(this.id(), questionId, dto);
      }
      this.selectedQuestionId.set(null);
      this.questionForm().reset(this.questionModel(null));
      this.quiz.reload();
    } catch (failure) {
      this.questionError.set(serverMessage(failure, 'Could not save the question.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async removeQuestion(questionId: string): Promise<void> {
    this.pending.set(true);
    this.questionError.set(null);
    try {
      await this.quizAuthoring.removeQuestion(this.id(), questionId);
      if (this.selectedQuestionId() === questionId) {
        this.selectedQuestionId.set(null);
        this.questionForm().reset(this.questionModel(null));
      }
      this.quiz.reload();
    } catch (failure) {
      this.questionError.set(serverMessage(failure, 'Could not remove the question.'));
    } finally {
      this.pending.set(false);
    }
  }

  // The question form always has four answer fields; unused ones stay empty.
  private questionModel(question: QuestionDto | null): { text: string; answers: AnswerDto[] } {
    const answers: AnswerDto[] = [];
    for (let index = 0; index < MAX_ANSWERS; index++) {
      const answer = question?.answers[index];
      answers.push({ text: answer?.text ?? '', isCorrect: answer?.isCorrect ?? false });
    }
    return { text: question?.text ?? '', answers };
  }
}
