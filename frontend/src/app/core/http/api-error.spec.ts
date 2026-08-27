import { HttpErrorResponse } from '@angular/common/http';
import { parseApiError } from './api-error';

describe('parseApiError', () => {
  it('reads the backend error envelope', () => {
    const error = new HttpErrorResponse({
      status: 400,
      error: {
        error: {
          code: 'INSUFFICIENT_STOCK',
          message: 'One or more titles do not have enough available copies for this loan.',
          details: [{ bookId: '1', title: 'Dune', available: 0 }]
        }
      }
    });

    const parsed = parseApiError(error);
    expect(parsed.code).toBe('INSUFFICIENT_STOCK');
    expect(parsed.details?.length).toBe(1);
    expect(parsed.details?.[0].title).toBe('Dune');
  });

  it('falls back when the envelope is missing', () => {
    const error = new HttpErrorResponse({ status: 500, statusText: 'Server Error' });
    const parsed = parseApiError(error);
    expect(parsed.code).toBe('UNKNOWN');
  });
});
