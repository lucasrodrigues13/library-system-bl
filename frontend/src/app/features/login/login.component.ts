import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AuthApiService } from '../../core/api/auth-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { parseApiError } from '../../core/http/api-error';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule],
  template: `
    <main class="login-page">
      <mat-card appearance="outlined" class="login-card">
        <div class="login-banner">
          <mat-icon>menu_book</mat-icon>
          <div>
            <h1>Library System</h1>
            <p>Sign in with a seeded demo account</p>
          </div>
        </div>
        <mat-card-content>
          @if (error) {
            <p class="error" role="alert">{{ error }}</p>
          }
          <form [formGroup]="form" (ngSubmit)="submit()">
            <mat-form-field appearance="outline" class="full">
              <mat-label>Email</mat-label>
              <input matInput type="email" formControlName="email" autocomplete="username" />
            </mat-form-field>
            <mat-form-field appearance="outline" class="full">
              <mat-label>Password</mat-label>
              <input matInput type="password" formControlName="password" autocomplete="current-password" />
            </mat-form-field>
            <button mat-flat-button class="full" type="submit" [disabled]="form.invalid || loading">
              {{ loading ? 'Signing in…' : 'Sign in' }}
            </button>
          </form>
          <section class="hints" aria-label="Seeded credentials">
            <h3>Demo credentials</h3>
            <p><strong>Admin:</strong> admin&#64;library.local / Admin123!</p>
            <p><strong>Client:</strong> alice&#64;library.local / Alice123!</p>
            <p>Also seeded: bob&#64;library.local / Bob123! and carol&#64;library.local / Carol123!</p>
          </section>
        </mat-card-content>
      </mat-card>
    </main>
  `,
  styles: [`
    .login-page {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 24px;
      background: #161616;
    }
    .login-card { width: min(480px, 100%); overflow: hidden; }
    .login-banner {
      display: flex;
      gap: 12px;
      align-items: center;
      background: #31c2a0;
      color: #fff;
      padding: 20px 24px;
    }
    .login-banner h1 { margin: 0; font-size: 1.5rem; font-weight: 500; }
    .login-banner p { margin: 4px 0 0; font-size: 14px; }
    .login-banner mat-icon { font-size: 36px; width: 36px; height: 36px; }
    mat-card-content { padding-top: 20px; }
    .full { width: 100%; display: block; margin-bottom: 8px; }
    .hints { margin-top: 16px; font-size: 14px; color: var(--lib-muted); }
    .hints h3 { margin: 0 0 8px; color: var(--lib-black); }
  `]
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(AuthApiService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  loading = false;
  error = '';
  form = this.fb.nonNullable.group({
    email: ['admin@library.local', [Validators.required, Validators.email]],
    password: ['Admin123!', Validators.required]
  });

  submit(): void {
    if (this.form.invalid) {
      return;
    }

    this.loading = true;
    this.error = '';
    const { email, password } = this.form.getRawValue();
    this.api.login(email, password).subscribe({
      next: (session) => {
        this.auth.setSession(session);
        void this.router.navigate(['/books']);
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        this.error = parseApiError(err).message;
      }
    });
  }
}
