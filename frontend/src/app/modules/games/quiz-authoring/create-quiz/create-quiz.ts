import { Component, inject, signal } from '@angular/core';
import { FormField, form, max, min, required } from '@angular/forms/signals';
import { Router } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { QuizAuthoring } from '../quiz-authoring';

@Component({
  imports: [FormField],
  selector: 'app-create-quiz',
  styleUrl: './create-quiz.scss',
  templateUrl: './create-quiz.html',
})
export class CreateQuiz {
  private readonly quizAuthoring = inject(QuizAuthoring);
  private readonly router = inject(Router);

  protected readonly form = form(
    signal({ title: '', description: '', experiencePoints: 10 }),
    (path) => {
      required(path.title, { message: 'Title is required.' });
      required(path.experiencePoints, { message: 'Experience points are required.' });
      min(path.experiencePoints, 1, { message: 'Experience points must be at least 1.' });
      max(path.experiencePoints, 5000, { message: 'Experience points must be at most 5000.' });
    },
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      const id = await this.quizAuthoring.create({
        title: this.form.title().value(),
        description: this.form.description().value(),
        experiencePoints: this.form.experiencePoints().value(),
      });
      await this.router.navigate(['/games/quizzes', id]);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not create the quiz.'));
    } finally {
      this.pending.set(false);
    }
  }
}
