import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { AuthService } from '../../core/auth/auth.service';
import { BooksComponent } from './books.component';

describe('BooksComponent', () => {
  function setup(isAdmin: boolean) {
    TestBed.configureTestingModule({
      imports: [BooksComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideNoopAnimations(),
        {
          provide: AuthService,
          useValue: {
            isAdmin: () => isAdmin,
            currentUser: () => ({ id: '1', name: 'Admin', email: 'admin@library.local', role: isAdmin ? 'Admin' : 'Client' })
          }
        }
      ]
    });

    const fixture = TestBed.createComponent(BooksComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http.expectOne((req) => req.url.includes('/api/v1/books')).flush([]);
    fixture.detectChanges();
    return fixture;
  }

  it('shows an Add button and keeps the form off the list page for admins', () => {
    const fixture = setup(true);
    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Add');
    expect(text).not.toContain('Add book');
    expect(text).not.toContain('Edit book');
    expect(fixture.nativeElement.querySelector('.add-button')).toBeTruthy();
  });

  it('does not show the Add button for clients', () => {
    const fixture = setup(false);
    expect(fixture.nativeElement.querySelector('.add-button')).toBeNull();
  });
});
