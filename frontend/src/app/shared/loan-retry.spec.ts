import { removeUnavailableTitles } from './loan-retry';

describe('removeUnavailableTitles', () => {
  it('removes only unavailable titles so the admin can retry', () => {
    const remaining = removeUnavailableTitles(
      ['available', 'gone', 'also-available'],
      [{ bookId: 'gone', title: 'Out of Print Tales', available: 0 }]
    );

    expect(remaining).toEqual(['available', 'also-available']);
  });

  it('keeps the original selection when there are no details', () => {
    expect(removeUnavailableTitles(['a', 'b'], undefined)).toEqual(['a', 'b']);
  });
});
