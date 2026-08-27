import { HttpErrorResponse } from '@angular/common/http';
import { ApiError } from '../models';

export function parseApiError(error: HttpErrorResponse): ApiError {
  const body = error.error as { error?: ApiError } | null;
  if (body?.error?.code) {
    return {
      code: body.error.code,
      message: body.error.message,
      details: body.error.details ?? []
    };
  }

  if (error.status === 401) {
    return { code: 'UNAUTHORIZED', message: 'Authentication is required.' };
  }

  if (error.status === 403) {
    return { code: 'FORBIDDEN', message: 'You do not have permission to perform this action.' };
  }

  return {
    code: 'UNKNOWN',
    message: error.message || 'An unexpected error occurred.'
  };
}
