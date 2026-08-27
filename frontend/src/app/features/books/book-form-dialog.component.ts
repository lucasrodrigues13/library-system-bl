import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { BooksApiService } from '../../core/api/books-api.service';
import { BookDto } from '../../core/models';
import { parseApiError } from '../../core/http/api-error';

export interface BookFormDialogData {
  book?: BookDto;
}

@Component({
  selector: 'app-book-form-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule],
  template: `
    <h2 mat-dialog-title>{{ data.book ? 'Edit book' : 'Add book' }}</h2>
    <mat-dialog-content>
      @if (error) {
        <p class="error" role="alert">{{ error }}</p>
      }
      <form class="form-grid" [formGroup]="form" (ngSubmit)="save()">
        <mat-form-field appearance="outline" class="full">
          <mat-label>Title</mat-label>
          <input matInput formControlName="title" />
        </mat-form-field>
        <mat-form-field appearance="outline" class="full">
          <mat-label>Author</mat-label>
          <input matInput formControlName="author" />
        </mat-form-field>
        <mat-form-field appearance="outline" class="full">
          <mat-label>ISBN</mat-label>
          <input matInput formControlName="isbn" />
        </mat-form-field>
        <mat-form-field appearance="outline" class="full">
          <mat-label>Total units</mat-label>
          <input matInput type="number" formControlName="totalQuantity" />
          <mat-hint>Copies the library owns.</mat-hint>
        </mat-form-field>
        <mat-form-field appearance="outline" class="full">
          <mat-label>Available</mat-label>
          <input matInput type="number" formControlName="quantity" />
          <mat-hint>Copies on the shelf. Used when creating a loan.</mat-hint>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-stroked-button type="button" (click)="dialogRef.close(false)" [disabled]="saving">Cancel</button>
      <button mat-flat-button type="button" (click)="save()" [disabled]="form.invalid || saving">
        {{ saving ? 'Saving…' : 'Save' }}
      </button>
    </mat-dialog-actions>
  `
})
export class BookFormDialogComponent {
  readonly data = inject<BookFormDialogData>(MAT_DIALOG_DATA);
  readonly dialogRef = inject(MatDialogRef<BookFormDialogComponent, boolean>);
  private readonly api = inject(BooksApiService);
  private readonly fb = inject(FormBuilder);

  error = '';
  saving = false;
  form = this.fb.nonNullable.group({
    title: [this.data.book?.title ?? '', Validators.required],
    author: [this.data.book?.author ?? '', Validators.required],
    isbn: [this.data.book?.isbn ?? '', Validators.required],
    totalQuantity: [this.data.book?.totalQuantity ?? 1, [Validators.required, Validators.min(0)]],
    quantity: [this.data.book?.quantity ?? 1, [Validators.required, Validators.min(0)]]
  });

  save(): void {
    if (this.form.invalid) {
      return;
    }

    this.saving = true;
    this.error = '';
    const value = this.form.getRawValue();
    if (value.quantity > value.totalQuantity) {
      this.saving = false;
      this.error = 'Available copies cannot exceed total units.';
      return;
    }
    const request = this.data.book
      ? this.api.update(this.data.book.id, value)
      : this.api.create(value);

    request.subscribe({
      next: () => this.dialogRef.close(true),
      error: (err: HttpErrorResponse) => {
        this.saving = false;
        this.error = parseApiError(err).message;
      }
    });
  }
}
