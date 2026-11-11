import { httpResource } from '@angular/common/http';
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { QuizDto } from '../../api/games-api-types';

@Component({
  imports: [RouterLink],
  selector: 'app-my-quizzes',
  styleUrl: './my-quizzes.scss',
  templateUrl: './my-quizzes.html',
})
export class MyQuizzes {
  protected readonly quizzes = httpResource<QuizDto[]>(() => '/api/games/quizzes/mine');
}
