import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  imports: [RouterLink],
  selector: 'app-games-home',
  template: `
    <div class="stack">
      <h1>Games</h1>
      <p><a routerLink="/games/mine">My quizzes</a></p>
      <p><a routerLink="/games/create">New quiz</a></p>
    </div>
  `,
})
export class GamesHome {}
