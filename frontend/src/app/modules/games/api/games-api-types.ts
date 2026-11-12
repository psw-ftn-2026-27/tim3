export type QuizStatus = 'Draft' | 'Published' | 'Retired' | 'Archived';

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
