import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-gift-card-load-error',
  imports: [RouterLink],
  templateUrl: './gift-card-load-error.html',
  host: { style: 'display: block' },
})
export class GiftCardLoadError {
  readonly failure = input.required<unknown>();
  readonly title = input.required<string>();
  readonly busy = input(false);
  readonly retry = output<void>();

  protected readonly status = computed(() => {
    const error = this.failure();
    return error instanceof HttpErrorResponse ? error.status : null;
  });
}
