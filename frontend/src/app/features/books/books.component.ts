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
import { BooksApiService } from '../../core/api/books-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { BookDto } from '../../core/models';
import { parseApiError } from '../../core/http/api-error';
import { ConfirmDialogComponent } from '../../shared/confirm-dialog.component';
import { confirmDialogConfig, formDialogConfig } from '../../shared/dialog';
import { BookFormDialogComponent } from './book-form-dialog.component';

@Component({
  selector: 'app-books',
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
    <p class="breadcrumb">Library / Books</p>
    <h1 class="page-title">
      {{ auth.isAdmin() ? 'Book catalog' : 'Available books' }}
      <span class="muted">{{ auth.isAdmin() ? 'Add titles and keep available stock up to date.' : 'Only titles with remaining copies are shown.' }}</span>
    </h1>

    <mat-card class="toolbar-card">
      <mat-card-content class="toolbar-card-content">
        <mat-form-field appearance="outline" class="search-field">
          <mat-label>Search</mat-label>
          <mat-icon matPrefix>search</mat-icon>
          <input matInput (input)="query = $any($event.target).value" />
        </mat-form-field>
        @if (auth.isAdmin()) {
          <button mat-flat-button class="add-button" type="button" (click)="openForm()">
            <mat-icon>add</mat-icon>
            Add
          </button>
        }
      </mat-card-content>
    </mat-card>

    @if (error) {
      <p class="error" role="alert">{{ error }}</p>
    }

    <mat-card class="table-card">
      <mat-card-content>
        @if (loading) {
          <div class="state"><mat-spinner diameter="32" /></div>
        } @else if (!filteredBooks.length) {
          <div class="state">No books to display.</div>
        } @else {
          <table mat-table [dataSource]="filteredBooks" class="full-table">
            <ng-container matColumnDef="title">
              <th mat-header-cell *matHeaderCellDef>Title</th>
              <td mat-cell *matCellDef="let row">{{ row.title }}</td>
            </ng-container>
            <ng-container matColumnDef="author">
              <th mat-header-cell *matHeaderCellDef>Author</th>
              <td mat-cell *matCellDef="let row">{{ row.author }}</td>
            </ng-container>
            <ng-container matColumnDef="isbn">
              <th mat-header-cell *matHeaderCellDef>ISBN</th>
              <td mat-cell *matCellDef="let row">{{ row.isbn }}</td>
            </ng-container>
            <ng-container matColumnDef="totalQuantity">
              <th mat-header-cell *matHeaderCellDef>Total</th>
              <td mat-cell *matCellDef="let row">{{ row.totalQuantity }}</td>
            </ng-container>
            <ng-container matColumnDef="quantity">
              <th mat-header-cell *matHeaderCellDef>Available</th>
              <td mat-cell *matCellDef="let row">
                <span class="status-chip" [class.in-stock]="row.quantity > 0">{{ row.quantity }}</span>
              </td>
            </ng-container>
            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef>Actions</th>
              <td mat-cell *matCellDef="let row">
                <div class="row-actions">
                  <button mat-icon-button type="button" aria-label="Edit book" (click)="openForm(row)">
                    <mat-icon>edit</mat-icon>
                  </button>
                  <button mat-icon-button type="button" aria-label="Delete book" (click)="confirmDelete(row)">
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
export class BooksComponent implements OnInit {
  private readonly api = inject(BooksApiService);
  readonly auth = inject(AuthService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  books: BookDto[] = [];
  query = '';
  loading = false;
  error = '';
  displayedColumns = ['title', 'author', 'isbn', 'totalQuantity', 'quantity', 'actions'];

  get filteredBooks(): BookDto[] {
    const q = this.query.trim().toLowerCase();
    if (!q) {
      return this.books;
    }
    return this.books.filter((book) =>
      [book.title, book.author, book.isbn].some((value) => value.toLowerCase().includes(q))
    );
  }

  ngOnInit(): void {
    if (!this.auth.isAdmin()) {
      this.displayedColumns = ['title', 'author', 'isbn', 'totalQuantity', 'quantity'];
    }
    this.reload();
  }

  openForm(book?: BookDto): void {
    this.dialog.open(BookFormDialogComponent, { ...formDialogConfig, data: { book } })
      .afterClosed()
      .subscribe((saved) => {
        if (saved) {
          this.snackBar.open(book ? 'Book updated.' : 'Book added.', 'OK', { duration: 3000, panelClass: 'snack-success' });
          this.reload();
        }
      });
  }

  confirmDelete(book: BookDto): void {
    this.dialog.open(ConfirmDialogComponent, {
      ...confirmDialogConfig,
      data: { message: `Are you sure you want to delete "${book.title}"?` }
    }).afterClosed().subscribe((confirmed) => {
      if (!confirmed) {
        return;
      }
      this.api.delete(book.id).subscribe({
        next: () => {
          this.snackBar.open('Book deleted.', 'OK', { duration: 3000, panelClass: 'snack-success' });
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
      next: (books) => {
        this.books = books;
        this.loading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        this.error = parseApiError(err).message;
      }
    });
  }
}
