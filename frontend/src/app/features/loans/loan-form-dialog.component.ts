import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { LoansApiService } from '../../core/api/loans-api.service';
import { UsersApiService } from '../../core/api/users-api.service';
import { BooksApiService } from '../../core/api/books-api.service';
import { BookDto, ErrorDetail, UserDto } from '../../core/models';
import { parseApiError } from '../../core/http/api-error';
import { removeUnavailableTitles } from '../../shared/loan-retry';

@Component({
  selector: 'app-loan-form-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatSelectModule],
  template: `
    <h2 mat-dialog-title>Create loan</h2>
    <mat-dialog-content>
      <p class="muted">Select a client and up to 3 titles. Only one unit of each title is allowed.</p>
      @if (error) {
        <p class="error" role="alert">{{ error }}</p>
      }
      @if (stockIssues.length) {
        <div class="error">
          <strong>Insufficient stock</strong>
          <p>Remove the unavailable titles and save again. The loan is not stored until every selected title is available.</p>
          <ul>
            @for (issue of stockIssues; track issue.bookId) {
              <li>{{ issue.title }} (available: {{ issue.available }})</li>
            }
          </ul>
          <button mat-stroked-button type="button" (click)="removeUnavailable()">Remove unavailable titles</button>
        </div>
      }
      <form class="form-grid" [formGroup]="form" (ngSubmit)="save()">
        <mat-form-field appearance="outline" class="full">
          <mat-label>Borrower</mat-label>
          <mat-select formControlName="borrowerId">
            @for (user of clients; track user.id) {
              <mat-option [value]="user.id">{{ user.name }}</mat-option>
            }
          </mat-select>
        </mat-form-field>
        <mat-form-field appearance="outline" class="full">
          <mat-label>Titles (max 3)</mat-label>
          <mat-select formControlName="bookIds" multiple>
            @for (book of books; track book.id) {
              <mat-option [value]="book.id" [disabled]="isDisabled(book.id)">
                {{ book.title }} ({{ book.quantity }} available of {{ book.totalQuantity }})
              </mat-option>
            }
          </mat-select>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-stroked-button type="button" (click)="dialogRef.close(false)" [disabled]="saving">Cancel</button>
      <button mat-flat-button type="button" (click)="save()" [disabled]="form.invalid || saving">
        {{ saving ? 'Saving…' : 'Save loan' }}
      </button>
    </mat-dialog-actions>
  `
})
export class LoanFormDialogComponent implements OnInit {
  readonly dialogRef = inject(MatDialogRef<LoanFormDialogComponent, boolean>);
  private readonly loansApi = inject(LoansApiService);
  private readonly usersApi = inject(UsersApiService);
  private readonly booksApi = inject(BooksApiService);
  private readonly fb = inject(FormBuilder);

  clients: UserDto[] = [];
  books: BookDto[] = [];
  stockIssues: ErrorDetail[] = [];
  error = '';
  saving = false;
  form = this.fb.nonNullable.group({
    borrowerId: ['', Validators.required],
    bookIds: [[] as string[], [Validators.required, Validators.minLength(1)]]
  });

  ngOnInit(): void {
    this.usersApi.list().subscribe((users) => this.clients = users.filter((user) => user.role === 'Client'));
    this.booksApi.list().subscribe((books) => this.books = books);
  }

  isDisabled(bookId: string): boolean {
    const selected = this.form.controls.bookIds.value;
    return selected.length >= 3 && !selected.includes(bookId);
  }

  removeUnavailable(): void {
    const remaining = removeUnavailableTitles(this.form.controls.bookIds.value, this.stockIssues);
    this.form.controls.bookIds.setValue(remaining);
    this.stockIssues = [];
    this.error = remaining.length
      ? 'Unavailable titles were removed. Save again to register the loan.'
      : 'All selected titles were unavailable. Choose different titles.';
  }

  save(): void {
    if (this.form.invalid) {
      return;
    }

    this.saving = true;
    this.error = '';
    const { borrowerId, bookIds } = this.form.getRawValue();
    this.loansApi.create(borrowerId, bookIds).subscribe({
      next: () => this.dialogRef.close(true),
      error: (err: HttpErrorResponse) => {
        this.saving = false;
        const parsed = parseApiError(err);
        this.error = parsed.message;
        this.stockIssues = parsed.code === 'INSUFFICIENT_STOCK' ? parsed.details ?? [] : [];
      }
    });
  }
}
