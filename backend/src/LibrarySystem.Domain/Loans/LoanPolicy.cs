using LibrarySystem.Domain.Common;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Enums;

namespace LibrarySystem.Domain.Loans;

public static class LoanPolicy
{
    public const int MaxTitlesPerLoan = 3;
    public const int UnitsPerTitle = 1;

    public static Result ValidateCreate(
        IReadOnlyList<Guid> bookIds,
        IReadOnlyDictionary<Guid, Book> booksById)
    {
        var shape = ValidateShape(bookIds);
        if (shape.IsFailure)
        {
            return shape;
        }

        return ValidateStock(bookIds, booksById);
    }

    public static Result ValidateShape(IReadOnlyList<Guid>? bookIds)
    {
        if (bookIds is null || bookIds.Count == 0)
        {
            return Result.Failure(Errors.LoanEmpty());
        }

        if (bookIds.Count > MaxTitlesPerLoan)
        {
            return Result.Failure(Errors.LoanTooManyTitles());
        }

        if (bookIds.Distinct().Count() != bookIds.Count)
        {
            return Result.Failure(Errors.LoanDuplicateTitle());
        }

        return Result.Success();
    }

    public static Result ValidateStock(
        IReadOnlyList<Guid> bookIds,
        IReadOnlyDictionary<Guid, Book> booksById)
    {
        var insufficient = new List<ErrorDetail>();

        foreach (var id in bookIds)
        {
            if (!booksById.TryGetValue(id, out var book))
            {
                return Result.Failure(Errors.BookNotFound(id));
            }

            if (book.Quantity < UnitsPerTitle)
            {
                insufficient.Add(new ErrorDetail(book.Id, book.Title, book.Quantity));
            }
        }

        if (insufficient.Count > 0)
        {
            return Result.Failure(Errors.InsufficientStock(insufficient));
        }

        return Result.Success();
    }

    public static Result ValidateReturn(Loan loan)
    {
        if (loan.Status == LoanStatus.Returned)
        {
            return Result.Failure(Errors.LoanAlreadyReturned());
        }

        return Result.Success();
    }
}
