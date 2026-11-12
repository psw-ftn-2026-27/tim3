import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../core/auth/auth';

@Component({
  imports: [RouterLink],
  selector: 'app-games-home',
  styleUrl: './games-home.scss',
  templateUrl: './games-home.html',
})
export class GamesHome {
  private readonly auth = inject(Auth);

  // The tab the user is looking at; it starts on quizzes on every visit.
  protected readonly tab = signal<'quizzes' | 'social'>('quizzes');

  protected readonly isAdministrator = computed(
    () => this.auth.user()?.roles.includes('administrator') ?? false,
  );
}
