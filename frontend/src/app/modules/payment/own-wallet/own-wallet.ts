import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { DepositEcDto } from '../api/payment-api-types';

const BASE_URL = '/api/payment/wallet';

@Injectable({ providedIn: 'root' })
export class OwnWallet {
  private readonly http = inject(HttpClient);

  async ensureWallet(): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/me`, null));
  }

  async deposit(userId: string, dto: DepositEcDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${userId}/deposits`, dto));
  }

  async reverse(userId: string, depositId: string): Promise<void> {
    await firstValueFrom(
      this.http.post<void>(`${BASE_URL}/${userId}/deposits/${depositId}/reverse`, null),
    );
  }
}
