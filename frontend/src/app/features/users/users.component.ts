import { Component, OnInit, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { UsersApiService } from '../../core/api/users-api.service';
import { UserDto } from '../../core/models';
import { parseApiError } from '../../core/http/api-error';
import { ConfirmDialogComponent } from '../../shared/confirm-dialog.component';
import { confirmDialogConfig, formDialogConfig } from '../../shared/dialog';
import { UserFormDialogComponent } from './user-form-dialog.component';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [
    MatTableModule,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatFormFieldModule,
    MatInputModule
  ],
  template: `
    <p class="breadcrumb">Library / Users</p>
    <h1 class="page-title">
      Users
      <span class="muted">Register members and keep admin accounts in one place.</span>
    </h1>

    <mat-card class="toolbar-card">
      <mat-card-content class="toolbar-card-content">
        <mat-form-field appearance="outline" class="search-field">
          <mat-label>Search</mat-label>
          <mat-icon matPrefix>search</mat-icon>
          <input matInput (input)="query = $any($event.target).value" />
        </mat-form-field>
        <button mat-flat-button class="add-button" type="button" (click)="openForm()">
          <mat-icon>add</mat-icon>
          Add
        </button>
      </mat-card-content>
    </mat-card>

    @if (error) {
      <p class="error" role="alert">{{ error }}</p>
    }

    <mat-card class="table-card">
      <mat-card-content>
        @if (loading) {
          <div class="state"><mat-spinner diameter="32" /></div>
        } @else if (!filteredUsers.length) {
          <div class="state">No users to display.</div>
        } @else {
          <table mat-table [dataSource]="filteredUsers" class="full-table">
            <ng-container matColumnDef="name">
              <th mat-header-cell *matHeaderCellDef>Name</th>
              <td mat-cell *matCellDef="let row">{{ row.name }}</td>
            </ng-container>
            <ng-container matColumnDef="email">
              <th mat-header-cell *matHeaderCellDef>Email</th>
              <td mat-cell *matCellDef="let row">{{ row.email }}</td>
            </ng-container>
            <ng-container matColumnDef="role">
              <th mat-header-cell *matHeaderCellDef>Role</th>
              <td mat-cell *matCellDef="let row">
                <span class="status-chip" [class.admin]="row.role === 'Admin'">{{ row.role }}</span>
              </td>
            </ng-container>
            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef>Actions</th>
              <td mat-cell *matCellDef="let row">
                <div class="row-actions">
                  <button mat-icon-button type="button" aria-label="Edit user" (click)="openForm(row)">
                    <mat-icon>edit</mat-icon>
                  </button>
                  <button mat-icon-button type="button" aria-label="Delete user" (click)="confirmDelete(row)">
                    <mat-icon>delete</mat-icon>
                  </button>
                </div>
              </td>
            </ng-container>
            <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
          </table>
        }
      </mat-card-content>
    </mat-card>
  `
})
export class UsersComponent implements OnInit {
  private readonly api = inject(UsersApiService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  users: UserDto[] = [];
  query = '';
  loading = false;
  error = '';
  displayedColumns = ['name', 'email', 'role', 'actions'];

  get filteredUsers(): UserDto[] {
    const q = this.query.trim().toLowerCase();
    if (!q) {
      return this.users;
    }
    return this.users.filter((user) =>
      [user.name, user.email, user.role].some((value) => value.toLowerCase().includes(q))
    );
  }

  ngOnInit(): void {
    this.reload();
  }

  openForm(user?: UserDto): void {
    this.dialog.open(UserFormDialogComponent, { ...formDialogConfig, data: { user } })
      .afterClosed()
      .subscribe((saved) => {
        if (saved) {
          this.snackBar.open(user ? 'User updated.' : 'User added.', 'OK', { duration: 3000, panelClass: 'snack-success' });
          this.reload();
        }
      });
  }

  confirmDelete(user: UserDto): void {
    this.dialog.open(ConfirmDialogComponent, {
      ...confirmDialogConfig,
      data: { message: `Are you sure you want to delete "${user.name}"?` }
    }).afterClosed().subscribe((confirmed) => {
      if (!confirmed) {
        return;
      }
      this.api.delete(user.id).subscribe({
        next: () => {
          this.snackBar.open('User deleted.', 'OK', { duration: 3000, panelClass: 'snack-success' });
          this.reload();
        },
        error: (err: HttpErrorResponse) => this.error = parseApiError(err).message
      });
    });
  }

  private reload(): void {
    this.loading = true;
    this.error = '';
    this.api.list().subscribe({
      next: (users) => {
        this.users = users;
        this.loading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        this.error = parseApiError(err).message;
      }
    });
  }
}
