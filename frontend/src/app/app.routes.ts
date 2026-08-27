import { Routes } from '@angular/router';
import { adminGuard, authGuard } from './core/auth/auth.guard';
import { LoginComponent } from './features/login/login.component';
import { BooksComponent } from './features/books/books.component';
import { UsersComponent } from './features/users/users.component';
import { LoansComponent } from './features/loans/loans.component';
import { ShellComponent } from './layouts/shell.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'books' },
      { path: 'books', component: BooksComponent },
      { path: 'users', component: UsersComponent, canActivate: [adminGuard] },
      { path: 'loans', component: LoansComponent, canActivate: [adminGuard] }
    ]
  },
  { path: '**', redirectTo: 'books' }
];
