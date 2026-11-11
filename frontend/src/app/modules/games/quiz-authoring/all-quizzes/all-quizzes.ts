import { httpResource } from '@angular/common/http';
import { Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { QuizDto } from '../../api/games-api-types';

const PAGE_SIZE = 10;

@Component({
  imports: [RouterLink],
  selector: 'app-all-quizzes',
  styleUrl: './all-quizzes.scss',
  templateUrl: './all-quizzes.html',
})
export class AllQuizzes {
  protected readonly page = signal(1);

  protected readonly quizzes = httpResource<PageResult<QuizDto>>(
    () => `/api/games/quizzes?page=${this.page()}&pageSize=${PAGE_SIZE}`,
  );

  protected readonly hasPrevious = computed(() => this.page() > 1);

  protected readonly hasNext = computed(
    () => this.page() * PAGE_SIZE < (this.quizzes.value()?.totalCount ?? 0),
  );

  protected previous(): void {
    this.page.update((page) => page - 1);
  }

  protected next(): void {
    this.page.update((page) => page + 1);
  }
}
