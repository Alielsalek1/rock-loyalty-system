import { Injectable } from '@angular/core';
import { BehaviorSubject, catchError, map, tap, throwError } from 'rxjs';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../env';
import { Voucher, VouchersResponse } from './vouchers.interface';

@Injectable({
  providedIn: 'root',
})
export class VouchersService {
  vouchers: BehaviorSubject<Voucher[]> = new BehaviorSubject<Voucher[]>(null);
  constructor(private http: HttpClient) {}
  getVouchers(pageNumber: number, pageSize: number) {
    return this.http
      .get<VouchersResponse>(`${environment.apiUrl}/api/vouchers`, {
        params: new HttpParams()
          .append('pageNumber', pageNumber)
          .append('pageSize', pageSize),
      })
      .pipe(
        tap((response) => {
          this.vouchers.next(response.data.vouchers);
        })
      );
  }
}
