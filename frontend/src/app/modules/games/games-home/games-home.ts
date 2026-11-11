import { Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  imports: [RouterLink],
  selector: 'app-games-home',
  styleUrl: './games-home.scss',
  templateUrl: './games-home.html',
})
export class GamesHome {
  // The tab the user is looking at; it starts on quizzes on every visit.
  protected readonly tab = signal<'quizzes' | 'social'>('quizzes');
}
