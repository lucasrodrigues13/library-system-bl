import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { UserDto, UserRole } from '../models';

@Injectable({ providedIn: 'root' })
export class UsersApiService {
  private readonly baseUrl = `${environment.apiBaseUrl}/api/v1/users`;

  constructor(private readonly http: HttpClient) {}

  list() {
    return this.http.get<UserDto[]>(this.baseUrl);
  }

  create(body: { name: string; email: string; password: string; role: UserRole }) {
    return this.http.post<UserDto>(this.baseUrl, body);
  }

  update(id: string, body: { name: string; email: string; password?: string; role: UserRole }) {
    return this.http.put<UserDto>(`${this.baseUrl}/${id}`, body);
  }

  delete(id: string) {
    return this.http.delete(`${this.baseUrl}/${id}`);
  }
}
