import { Component, computed, input, output } from '@angular/core';

@Component({
  selector: 'app-wallet-pagination',
  templateUrl: './wallet-pagination.html',
  styleUrl: './wallet-pagination.scss',
})
export class WalletPagination {
  readonly label = input.required<string>();
  readonly page = input.required<number>();
  readonly pageSize = input.required<number>();
  readonly totalCount = input.required<number>();
  readonly disabled = input(false);

  readonly pageChange = output<number>();
  protected readonly lastPage = computed(() => Math.ceil(this.totalCount() / this.pageSize()));
}
