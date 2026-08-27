import { Component, ViewChild, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenav, MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { AuthService } from '../core/auth/auth.service';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatSidenavModule,
    MatListModule
  ],
  template: `
    <div class="shell">
      <mat-toolbar class="header">
        <button mat-icon-button type="button" class="menu-button" (click)="sidenav.toggle()" aria-label="Toggle menu">
          <mat-icon>menu</mat-icon>
        </button>
        <mat-icon class="brand-icon" aria-hidden="true">menu_book</mat-icon>
        <span class="brand">Library System</span>
        <span class="spacer"></span>
        <span class="avatar" aria-hidden="true">{{ initials }}</span>
        <span class="user">{{ auth.currentUser()?.name }} · {{ auth.currentUser()?.role }}</span>
        <button mat-flat-button type="button" class="logout" (click)="auth.logout()">
          <mat-icon>logout</mat-icon>
          Logout
        </button>
      </mat-toolbar>

      <mat-sidenav-container class="layout">
        <mat-sidenav #sidenav class="sidebar" [opened]="true" [mode]="'side'">
          <nav>
            <p class="nav-label">Library</p>
            <a mat-list-item routerLink="/books" routerLinkActive="active">
              <mat-icon matListItemIcon>auto_stories</mat-icon>
              <span matListItemTitle>Books</span>
            </a>
            @if (auth.isAdmin()) {
              <a mat-list-item routerLink="/users" routerLinkActive="active">
                <mat-icon matListItemIcon>group</mat-icon>
                <span matListItemTitle>Users</span>
              </a>
              <a mat-list-item routerLink="/loans" routerLinkActive="active">
                <mat-icon matListItemIcon>assignment</mat-icon>
                <span matListItemTitle>Loans</span>
              </a>
            }
          </nav>
        </mat-sidenav>

        <mat-sidenav-content class="content">
          <router-outlet />
        </mat-sidenav-content>
      </mat-sidenav-container>
    </div>
  `,
  styles: [`
    .shell { height: 100vh; display: flex; flex-direction: column; }
    .header {
      position: sticky;
      top: 0;
      z-index: 3;
      background: var(--lib-black);
      color: #fff;
      height: 64px;
    }
    .menu-button { margin-right: 4px; color: #fff; }
    .brand-icon { margin-right: 10px; color: var(--lib-green); }
    .brand { font-weight: 500; letter-spacing: 0.01em; }
    .spacer { flex: 1; }
    .avatar {
      width: 32px;
      height: 32px;
      border-radius: 50%;
      background: var(--lib-green);
      color: #fff;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      font-size: 12px;
      font-weight: 600;
      margin-right: 10px;
    }
    .user { font-size: 14px; margin-right: 12px; color: #d7efe8; }
    .logout {
      background: var(--lib-green) !important;
      color: var(--lib-black) !important;
      --mdc-filled-button-container-color: var(--lib-green);
      --mdc-filled-button-label-text-color: var(--lib-black);
      --mat-button-filled-container-color: var(--lib-green);
      --mat-button-filled-label-text-color: var(--lib-black);
    }
    .logout mat-icon { color: var(--lib-black); }
    .layout { flex: 1; }
    .sidebar {
      width: 240px;
      background: var(--lib-black);
      border: none;
      color: #fff;
    }
    .nav-label {
      margin: 20px 20px 8px;
      font-size: 11px;
      letter-spacing: 0.12em;
      text-transform: uppercase;
      color: #8fd9c6;
    }
    .sidebar a {
      color: #eee;
      margin: 4px 8px;
      border-radius: 8px;
      width: auto;
    }
    .sidebar a mat-icon { color: var(--lib-green); }
    .sidebar a.active {
      background: var(--lib-green);
      color: #fff;
    }
    .sidebar a.active mat-icon { color: #fff; }
    .content {
      background: var(--lib-surface);
      padding: 28px 32px 40px;
      min-height: 100%;
      box-sizing: border-box;
    }
    @media (max-width: 720px) {
      .user { display: none; }
      .content { padding: 16px; }
    }
  `]
})
export class ShellComponent {
  @ViewChild('sidenav') sidenav?: MatSidenav;
  readonly auth = inject(AuthService);

  get initials(): string {
    const name = this.auth.currentUser()?.name ?? '';
    return name.split(' ').map((part) => part[0]).join('').slice(0, 2).toUpperCase() || 'LS';
  }
}
