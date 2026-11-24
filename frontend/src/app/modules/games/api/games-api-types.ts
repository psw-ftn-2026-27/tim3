export type QuizStatus = 'Draft' | 'Published' | 'Retired' | 'Archived';
export type SocialChallengeStatus = 'Draft' | 'Published' | 'Retired' | 'Archived';

export interface SocialChallengeDto {
  id: string;
  name: string;
  status: SocialChallengeStatus;
  activePeriodStart: string;
  activePeriodEnd: string;
  peopleRequired: number;
  gatheringRadius: number;
  experienceGained: number;
  endMessage: string;
  latitude: number | null;
  longitude: number | null;
}

export interface CreateSocialChallengeDto {
  name: string;
  activePeriodStart: string;
  activePeriodEnd: string;
  peopleRequired: number;
  gatheringRadius: number;
  experienceGained: number;
  endMessage: string;
  latitude: number | null;
  longitude: number | null;
}

export interface UpdateSocialChallengeDto {
  name: string;
  activePeriodStart: string;
  activePeriodEnd: string;
  peopleRequired: number;
  gatheringRadius: number;
  experienceGained: number;
  endMessage: string;
  latitude: number | null;
  longitude: number | null;
}

export interface AnswerDto {
  text: string;
  isCorrect: boolean;
}

export interface LocationDto {
  latitude: number;
  longitude: number;
}

export interface QuestionDto {
  id: string;
  text: string;
  answers: AnswerDto[];
}

export interface QuizDto {
  id: string;
  authorId: string;
  title: string;
  description: string;
  experiencePoints: number;
  status: QuizStatus;
  location: LocationDto | null;
  questions: QuestionDto[];
}

export interface CreateQuizDto {
  title: string;
  description: string;
  experiencePoints: number;
}

export interface UpdateQuizDetailsDto {
  title: string;
  description: string;
  experiencePoints: number;
  location: LocationDto | null;
}

export interface QuestionInputDto {
  text: string;
  answers: AnswerDto[];
}

export interface LocationChallengeSummaryDto {
  id: string;
  name: string;
  experienceGained: number;
  hiddenPointCount: number;
}

export interface HiddenPointDto {
  id: string;
  photo: string;
  latitude: number;
  longitude: number;
}

export interface LocationChallengeDto {
  id: string;
  name: string;
  startLatitude: number;
  startLongitude: number;
  experienceGained: number;
  hiddenPoints: HiddenPointDto[];
}

export interface CreateHiddenPointDto {
  photo: string;
  latitude: number;
  longitude: number;
}

export interface CreateLocationChallengeDto {
  name: string;
  startLatitude: number;
  startLongitude: number;
  experienceGained: number;
  hiddenPoints: CreateHiddenPointDto[];
}

export interface UpdateLocationChallengeDto {
  name: string;
  startLatitude: number;
  startLongitude: number;
  experienceGained: number;
  hiddenPoints: CreateHiddenPointDto[];
}

export type LocationChallengeState = 'NotStarted' | 'Active' | 'Solved';

export interface LocationChallengeToSolveDto {
  id: string;
  name: string;
  startLatitude: number;
  startLongitude: number;
  experienceGained: number;
  state: LocationChallengeState;
}

export interface SearchCircleDto {
  centerLatitude: number;
  centerLongitude: number;
  radius: number;
}

export interface HiddenPointClueDto {
  id: string;
  photo: string;
  discovered: boolean;
  latitude: number | null;
  longitude: number | null;
}

export interface LocationChallengeHuntDto {
  id: string;
  name: string;
  experienceGained: number;
  state: LocationChallengeState;
  searchCircle: SearchCircleDto | null;
  hiddenPoints: HiddenPointClueDto[];
}

export interface PublishedQuizDto {
  id: string;
  title: string;
  description: string;
  experiencePoints: number;
  location: LocationDto;
  questionCount: number;
  isSolved: boolean;
  correctAnswerCount: number;
}

export interface PlayQuestionDto {
  id: string;
  position: number;
  text: string;
  answers: string[];
}

export interface LockedQuestionDto {
  id: string;
  position: number;
  text: string;
  answers: string[];
  chosenAnswerIndex: number;
}

export interface QuizPlayDto {
  quizId: string;
  title: string;
  pass: number;
  questionCount: number;
  currentQuestion: PlayQuestionDto | null;
  lastAnswerCorrect: boolean | null;
  lockedQuestions: LockedQuestionDto[];
  correctAnswerCount: number;
  experienceGained: number | null;
}

export interface SubmitAnswerDto {
  questionId: string;
  answerIndex: number;
}
