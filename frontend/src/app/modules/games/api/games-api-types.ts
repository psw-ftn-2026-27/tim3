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
}

export interface CreateSocialChallengeDto {
  name: string;
  activePeriodStart: string;
  activePeriodEnd: string;
  peopleRequired: number;
  gatheringRadius: number;
  experienceGained: number;
  endMessage: string;
}

export interface UpdateSocialChallengeDto {
  name: string;
  activePeriodStart: string;
  activePeriodEnd: string;
  peopleRequired: number;
  gatheringRadius: number;
  experienceGained: number;
  endMessage: string;
}

export type QuizStatus = 'Draft';

export interface AnswerDto {
  text: string;
  isCorrect: boolean;
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
}

export interface CreateQuestionDto {
  text: string;
  answers: AnswerDto[];
}

export interface UpdateQuestionDto {
  text: string;
  answers: AnswerDto[];
}
