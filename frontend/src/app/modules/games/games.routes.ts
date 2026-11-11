import { Routes } from '@angular/router';
import { GamesHome } from './games-home/games-home';
import { CreateQuiz } from './quiz-authoring/create-quiz/create-quiz';
import { EditQuiz } from './quiz-authoring/edit-quiz/edit-quiz';
import { MyQuizzes } from './quiz-authoring/my-quizzes/my-quizzes';

export const gamesRoutes: Routes = [
  { path: '', component: GamesHome },
  { path: 'create', component: CreateQuiz },
  { path: 'mine', component: MyQuizzes },
  { path: 'quizzes/:id', component: EditQuiz },
];
