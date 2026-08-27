import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { BookDto } from '../models';

@Injectable({ providedIn: 'root' })
export class BooksApiService {
  private readonly baseUrl = `${environment.apiBaseUrl}/api/v1/books`;

  constructor(private readonly http: HttpClient) {}

  list() {
    return this.http.get<BookDto[]>(this.baseUrl);
  }

  create(body: Omit<BookDto, 'id'>) {
    return this.http.post<BookDto>(this.baseUrl, body);
  }

  update(id: string, body: Omit<BookDto, 'id'>) {
    return this.http.put<BookDto>(`${this.baseUrl}/${id}`, body);
  }

  delete(id: string) {
    return this.http.delete(`${this.baseUrl}/${id}`);
  }
}
