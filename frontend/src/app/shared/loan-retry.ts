import { ErrorDetail } from '../core/models';

export function removeUnavailableTitles(selectedIds: string[], details: ErrorDetail[] | undefined): string[] {
  const failed = new Set((details ?? []).map((detail) => detail.bookId));
  return selectedIds.filter((id) => !failed.has(id));
}
