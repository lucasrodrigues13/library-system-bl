import { Component, inject } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { UsersApiService } from '../../core/api/users-api.service';
import { UserDto, UserRole } from '../../core/models';
import { parseApiError } from '../../core/http/api-error';

export interface UserFormDialogData {
  user?: UserDto;
}

function optionalMinLength(min: number) {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = (control.value as string | null)?.trim() ?? '';
    if (!value) {
      return null;
    }
    return value.length >= min ? null : { minlength: { requiredLength: min, actualLength: value.length } };
  };
}

@Component({
  selector: 'app-user-form-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatSelectModule],
  template: `
    <h2 mat-dialog-title>{{ data.user ? 'Edit user' : 'Add user' }}</h2>
    <mat-dialog-content>
      @if (error) {
        <p class="error" role="alert">{{ error }}</p>
      }
      <form class="form-grid" [formGroup]="form" (ngSubmit)="save()">
        <mat-form-field appearance="outline" class="full">
          <mat-label>Name</mat-label>
          <input matInput formControlName="name" />
        </mat-form-field>
        <mat-form-field appearance="outline" class="full">
          <mat-label>Email</mat-label>
          <input matInput formControlName="email" />
        </mat-form-field>
        <mat-form-field appearance="outline" class="full">
          <mat-label>Password</mat-label>
          <input matInput type="password" formControlName="password" autocomplete="new-password" />
          @if (data.user) {
            <mat-hint>Leave blank to keep the current password.</mat-hint>
          }
        </mat-form-field>
        <mat-form-field appearance="outline" class="full">
          <mat-label>Role</mat-label>
          <mat-select formControlName="role">
            <mat-option value="Client">Client</mat-option>
            <mat-option value="Admin">Admin</mat-option>
          </mat-select>
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
export class UserFormDialogComponent {
  readonly data = inject<UserFormDialogData>(MAT_DIALOG_DATA);
  readonly dialogRef = inject(MatDialogRef<UserFormDialogComponent, boolean>);
  private readonly api = inject(UsersApiService);
  private readonly fb = inject(FormBuilder);

  error = '';
  saving = false;
  form = this.fb.nonNullable.group({
    name: [this.data.user?.name ?? '', Validators.required],
    email: [this.data.user?.email ?? '', [Validators.required, Validators.email]],
    password: ['', this.data.user ? [optionalMinLength(8)] : [Validators.required, Validators.minLength(8)]],
    role: [(this.data.user?.role ?? 'Client') as UserRole, Validators.required]
  });

  save(): void {
    if (this.form.invalid) {
      return;
    }

    this.saving = true;
    this.error = '';
    const value = this.form.getRawValue();
    const request = this.data.user
      ? this.api.update(this.data.user.id, {
          name: value.name,
          email: value.email,
          role: value.role,
          password: value.password || undefined
        })
      : this.api.create({
          name: value.name,
          email: value.email,
          role: value.role,
          password: value.password
        });

    request.subscribe({
      next: () => this.dialogRef.close(true),
      error: (err: HttpErrorResponse) => {
        this.saving = false;
        this.error = parseApiError(err).message;
      }
    });
  }
}
