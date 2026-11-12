import { Routes } from '@angular/router';
import { GamesHome } from './games-home/games-home';
import { AllQuizzes } from './quiz-authoring/all-quizzes/all-quizzes';
import { CreateQuiz } from './quiz-authoring/create-quiz/create-quiz';
import { EditQuiz } from './quiz-authoring/edit-quiz/edit-quiz';
import { CreateSocialChallenge } from './social-challenge-authoring/create-social-challenge/create-social-challenge';
import { EditSocialChallenge } from './social-challenge-authoring/edit-social-challenge/edit-social-challenge';
import { SocialChallengeList } from './social-challenge-authoring/social-challenge-list/social-challenge-list';

export const gamesRoutes: Routes = [
  { path: '', component: GamesHome },
  { path: 'quizzes', component: AllQuizzes },
  { path: 'quizzes/create', component: CreateQuiz },
  { path: 'quizzes/:id', component: EditQuiz },
  { path: 'challenges', component: SocialChallengeList },
  { path: 'challenges/create', component: CreateSocialChallenge },
  { path: 'challenges/:id/edit', component: EditSocialChallenge },
];
