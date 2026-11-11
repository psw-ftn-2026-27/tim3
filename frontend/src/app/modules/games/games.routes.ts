import { Routes } from '@angular/router';
import { GamesHome } from './games-home/games-home';
import { AllQuizzes } from './quiz-authoring/all-quizzes/all-quizzes';
import { CreateQuiz } from './quiz-authoring/create-quiz/create-quiz';
import { EditQuiz } from './quiz-authoring/edit-quiz/edit-quiz';

export const gamesRoutes: Routes = [
  { path: '', component: GamesHome },
  { path: 'quizzes', component: AllQuizzes },
  { path: 'quizzes/create', component: CreateQuiz },
  { path: 'quizzes/:id', component: EditQuiz },
];
