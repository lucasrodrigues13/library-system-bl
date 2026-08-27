import { Injectable, computed, signal } from '@angular/core';
import { Router } from '@angular/router';
import { CurrentUser, LoginResponse, UserRole } from '../models';

const TOKEN_KEY = 'library.token';
const USER_KEY = 'library.user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly currentUserSignal = signal<CurrentUser | null>(readUser());
  readonly currentUser = this.currentUserSignal.asReadonly();
  readonly isAuthenticated = computed(() => this.currentUserSignal() !== null);
  readonly isAdmin = computed(() => this.currentUserSignal()?.role === 'Admin');

  constructor(private readonly router: Router) {}

  token(): string | null {
    return sessionStorage.getItem(TOKEN_KEY);
  }

  setSession(login: LoginResponse): void {
    const user: CurrentUser = {
      id: login.userId,
      name: login.name,
      email: login.email,
      role: login.role
    };
    sessionStorage.setItem(TOKEN_KEY, login.token);
    sessionStorage.setItem(USER_KEY, JSON.stringify(user));
    this.currentUserSignal.set(user);
  }

  hasRole(role: UserRole): boolean {
    return this.currentUserSignal()?.role === role;
  }

  logout(): void {
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(USER_KEY);
    this.currentUserSignal.set(null);
    void this.router.navigate(['/login']);
  }
}

function readUser(): CurrentUser | null {
  const raw = sessionStorage.getItem(USER_KEY);
  if (!raw) {
    return null;
  }

  try {
    return JSON.parse(raw) as CurrentUser;
  } catch {
    return null;
  }
}
