export type QuizStatus = 'Draft';

export interface LocationDto {
  latitude: number;
  longitude: number;
}

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
  location: LocationDto | null;
  status: QuizStatus;
  questions: QuestionDto[];
}

export interface CreateQuizDto {
  title: string;
  description: string;
  location: LocationDto | null;
}

export interface UpdateQuizDetailsDto {
  title: string;
  description: string;
  location: LocationDto | null;
}

export interface CreateQuestionDto {
  text: string;
  answers: AnswerDto[];
}

export interface UpdateQuestionDto {
  text: string;
  answers: AnswerDto[];
}
