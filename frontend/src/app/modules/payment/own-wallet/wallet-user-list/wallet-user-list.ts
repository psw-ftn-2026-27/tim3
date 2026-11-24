import { Component, input, output } from '@angular/core';
import { FieldTree, FormField } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { UserWalletSummaryDto } from '../../api/payment-api-types';
import { WalletPagination } from '../wallet-pagination/wallet-pagination';

@Component({
  selector: 'app-wallet-user-list',
  imports: [FormField, RouterLink, WalletPagination],
  templateUrl: './wallet-user-list.html',
  styleUrl: './wallet-user-list.scss',
})
export class WalletUserList {
  readonly searchField = input.required<FieldTree<string>>();
  readonly users = input<PageResult<UserWalletSummaryDto>>();
  readonly loading = input(false);
  readonly failed = input(false);
  readonly errorStatus = input<number | null>(null);
  readonly selectedId = input<string | null>(null);
  readonly disabled = input(false);
  readonly page = input.required<number>();
  readonly pageSize = input.required<number>();
  readonly searchRequested = output<Event>();
  readonly userSelected = output<UserWalletSummaryDto>();
  readonly pageChange = output<number>();
  readonly retry = output<void>();
}
