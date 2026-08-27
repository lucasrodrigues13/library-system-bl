import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { adminGuard, authGuard } from './auth.guard';
import { AuthService } from './auth.service';

describe('auth guards', () => {
  it('redirects unauthenticated users to login', () => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: { isAuthenticated: () => false, isAdmin: () => false } }
      ]
    });

    const result = TestBed.runInInjectionContext(() => authGuard({} as never, {} as never));
    expect(String(result)).toContain('/login');
  });

  it('blocks clients from admin routes', () => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([{ path: 'books', children: [] }]),
        { provide: AuthService, useValue: { isAuthenticated: () => true, isAdmin: () => false } }
      ]
    });

    const result = TestBed.runInInjectionContext(() => adminGuard({} as never, {} as never));
    expect(String(result)).toContain('/books');
  });

  it('allows admins through', () => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: { isAuthenticated: () => true, isAdmin: () => true } }
      ]
    });

    const result = TestBed.runInInjectionContext(() => adminGuard({} as never, {} as never));
    expect(result).toBeTrue();
  });
});
