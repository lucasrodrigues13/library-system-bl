import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { LoanDto } from '../models';

@Injectable({ providedIn: 'root' })
export class LoansApiService {
  private readonly baseUrl = `${environment.apiBaseUrl}/api/v1/loans`;

  constructor(private readonly http: HttpClient) {}

  list() {
    return this.http.get<LoanDto[]>(this.baseUrl);
  }

  create(borrowerId: string, bookIds: string[]) {
    return this.http.post<LoanDto>(this.baseUrl, { borrowerId, bookIds });
  }

  returnLoan(id: string) {
    return this.http.post<LoanDto>(`${this.baseUrl}/${id}/return`, {});
  }
}
