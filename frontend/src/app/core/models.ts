export type UserRole = 'Admin' | 'Client';
export type LoanStatus = 'Active' | 'Returned';

export interface LoginResponse {
  token: string;
  userId: string;
  name: string;
  email: string;
  role: UserRole;
}

export interface CurrentUser {
  id: string;
  name: string;
  email: string;
  role: UserRole;
}

export interface UserDto {
  id: string;
  name: string;
  email: string;
  role: UserRole;
  createdAtUtc: string;
}

export interface BookDto {
  id: string;
  title: string;
  author: string;
  isbn: string;
  totalQuantity: number;
  quantity: number;
}

export interface LoanItemDto {
  bookId: string;
  title: string;
  quantity: number;
}

export interface LoanDto {
  id: string;
  borrowerId: string;
  borrowerName: string;
  createdByAdminId: string;
  status: LoanStatus;
  createdAtUtc: string;
  returnedAtUtc?: string | null;
  items: LoanItemDto[];
}

export interface ErrorDetail {
  bookId: string;
  title: string;
  available: number;
}

export interface ApiError {
  code: string;
  message: string;
  details?: ErrorDetail[];
}
