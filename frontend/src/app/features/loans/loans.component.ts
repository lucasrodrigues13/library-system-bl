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
import { LoansApiService } from '../../core/api/loans-api.service';
import { LoanDto } from '../../core/models';
import { parseApiError } from '../../core/http/api-error';
import { ConfirmDialogComponent } from '../../shared/confirm-dialog.component';
import { confirmDialogConfig, formDialogConfig } from '../../shared/dialog';
import { LoanFormDialogComponent } from './loan-form-dialog.component';

@Component({
  selector: 'app-loans',
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
    <p class="breadcrumb">Library / Loans</p>
    <h1 class="page-title">
      Loans
      <span class="muted">Lend up to three available titles per member in one loan.</span>
    </h1>

    <mat-card class="toolbar-card">
      <mat-card-content class="toolbar-card-content">
        <mat-form-field appearance="outline" class="search-field">
          <mat-label>Search</mat-label>
          <mat-icon matPrefix>search</mat-icon>
          <input matInput (input)="query = $any($event.target).value" />
        </mat-form-field>
        <button mat-flat-button class="add-button" type="button" (click)="openCreate()">
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
        } @else if (!filteredLoans.length) {
          <div class="state">No loans yet.</div>
        } @else {
          <table mat-table [dataSource]="filteredLoans" class="full-table">
            <ng-container matColumnDef="borrower">
              <th mat-header-cell *matHeaderCellDef>Borrower</th>
              <td mat-cell *matCellDef="let row">{{ row.borrowerName }}</td>
            </ng-container>
            <ng-container matColumnDef="items">
              <th mat-header-cell *matHeaderCellDef>Titles</th>
              <td mat-cell *matCellDef="let row">{{ titleList(row) }}</td>
            </ng-container>
            <ng-container matColumnDef="status">
              <th mat-header-cell *matHeaderCellDef>Status</th>
              <td mat-cell *matCellDef="let row">
                <span class="status-chip" [class.active]="row.status === 'Active'">{{ row.status }}</span>
              </td>
            </ng-container>
            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef>Actions</th>
              <td mat-cell *matCellDef="let row">
                @if (row.status === 'Active') {
                  <button mat-stroked-button type="button" (click)="confirmReturn(row)">Return</button>
                }
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
export class LoansComponent implements OnInit {
  private readonly loansApi = inject(LoansApiService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  loans: LoanDto[] = [];
  query = '';
  loading = false;
  error = '';
  displayedColumns = ['borrower', 'items', 'status', 'actions'];

  get filteredLoans(): LoanDto[] {
    const q = this.query.trim().toLowerCase();
    if (!q) {
      return this.loans;
    }
    return this.loans.filter((loan) =>
      [loan.borrowerName, loan.status, this.titleList(loan)].some((value) => value.toLowerCase().includes(q))
    );
  }

  ngOnInit(): void {
    this.reload();
  }

  titleList(loan: LoanDto): string {
    return loan.items.map((item) => item.title).join(', ');
  }

  openCreate(): void {
    this.dialog.open(LoanFormDialogComponent, formDialogConfig)
      .afterClosed()
      .subscribe((saved) => {
        if (saved) {
          this.snackBar.open('Loan created.', 'OK', { duration: 3000, panelClass: 'snack-success' });
          this.reload();
        }
      });
  }

  confirmReturn(loan: LoanDto): void {
    this.dialog.open(ConfirmDialogComponent, {
      ...confirmDialogConfig,
      data: {
        title: 'Return loan',
        message: `Return the loan for ${loan.borrowerName} and restore stock for each title?`,
        confirmLabel: 'Return'
      }
    }).afterClosed().subscribe((confirmed) => {
      if (!confirmed) {
        return;
      }
      this.loansApi.returnLoan(loan.id).subscribe({
        next: () => {
          this.snackBar.open('Loan returned.', 'OK', { duration: 3000, panelClass: 'snack-success' });
          this.reload();
        },
        error: (err: HttpErrorResponse) => this.error = parseApiError(err).message
      });
    });
  }

  private reload(): void {
    this.loading = true;
    this.error = '';
    this.loansApi.list().subscribe({
      next: (loans) => {
        this.loans = loans;
        this.loading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        this.error = parseApiError(err).message;
      }
    });
  }
}
