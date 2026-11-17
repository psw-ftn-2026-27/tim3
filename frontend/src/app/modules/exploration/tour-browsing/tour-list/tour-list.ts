import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { PageResult } from '../../../../shared/api/page-result';
import { PublishedTourDto } from '../../api/exploration-api-types';
import { TourCard } from '../tour-card/tour-card';

@Component({
  imports: [FormField, RouterLink, TourCard],
  selector: 'app-tour-list',
  styleUrl: './tour-list.scss',
  templateUrl: './tour-list.html',
})
export class TourList {
  protected readonly auth = inject(Auth);

  // Only an explorer may author tours (the server answers 403 to anyone else), so only an
  // explorer is offered "Create tour" on an empty list.
  protected readonly isExplorer = computed(
    () => this.auth.user()?.roles.includes('explorer') ?? false,
  );

  protected readonly tours = httpResource<PageResult<PublishedTourDto>>(
    () => '/api/exploration/tours/published?page=1&pageSize=20',
  );

  protected readonly toursErrorStatus = computed(() => {
    const failure = this.tours.error();
    return failure instanceof HttpErrorResponse ? failure.status : null;
  });

  private readonly model = signal({ name: '', difficulty: '' });

  protected readonly form = form(this.model);

  protected readonly hasFilters = computed(
    () => this.model().name !== '' || this.model().difficulty !== '',
  );

  protected readonly visibleTours = computed<PublishedTourDto[]>(() => {
    const all = this.tours.value()?.items ?? [];
    const name = this.model().name.toLowerCase();
    const difficulty = this.model().difficulty;
    return all.filter(
      (tour) =>
        tour.name.toLowerCase().includes(name) &&
        (difficulty === '' || tour.difficulty === difficulty),
    );
  });

  protected clearFilters(): void {
    this.form().reset({ name: '', difficulty: '' });
  }
}
