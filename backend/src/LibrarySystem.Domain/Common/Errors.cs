namespace LibrarySystem.Domain.Common;

public static class Errors
{
    public static Error InvalidInput(string message) =>
        new("INVALID_INPUT", message);

    public static Error InvalidCredentials() =>
        new("INVALID_CREDENTIALS", "Invalid email or password.");

    public static Error Unauthorized() =>
        new("UNAUTHORIZED", "Authentication is required.");

    public static Error Forbidden() =>
        new("FORBIDDEN", "You do not have permission to perform this action.");

    public static Error NotFound(string entity) =>
        new("NOT_FOUND", $"{entity} was not found.");

    public static Error DuplicateEmail() =>
        new("DUPLICATE_EMAIL", "A user with this email already exists.");

    public static Error DuplicateIsbn() =>
        new("DUPLICATE_ISBN", "A book with this ISBN already exists.");

    public static Error LoanEmpty() =>
        new("LOAN_EMPTY", "Select at least one title.");

    public static Error LoanTooManyTitles() =>
        new("LOAN_TOO_MANY_TITLES", "A loan can include at most 3 titles.");

    public static Error LoanDuplicateTitle() =>
        new("LOAN_DUPLICATE_TITLE", "A loan can include only one unit of each title.");

    public static Error InsufficientStock(IReadOnlyList<ErrorDetail> details) =>
        new(
            "INSUFFICIENT_STOCK",
            "One or more titles do not have enough available copies for this loan.",
            details);

    public static Error BookNotFound(Guid id) =>
        new("NOT_FOUND", $"Book '{id}' was not found.");

    public static Error LoanAlreadyReturned() =>
        new("LOAN_ALREADY_RETURNED", "This loan has already been returned.");

    public static Error UserHasActiveLoans() =>
        new("USER_HAS_ACTIVE_LOANS", "Cannot delete a user with active loans.");

    public static Error BookHasActiveLoans() =>
        new("BOOK_HAS_ACTIVE_LOANS", "Cannot delete a book that is currently on loan.");

    public static Error BorrowerMustBeClient() =>
        new("INVALID_INPUT", "Loans can only be created for client users.");

    public static Error CannotDeleteSelf() =>
        new("INVALID_INPUT", "You cannot delete your own account.");
}
