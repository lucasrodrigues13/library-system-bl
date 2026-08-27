using LibrarySystem.Domain.Enums;

namespace LibrarySystem.Application.Loans;

public sealed class LoanDto
{
    public required Guid Id { get; init; }
    public required Guid BorrowerId { get; init; }
    public required string BorrowerName { get; init; }
    public required Guid CreatedByAdminId { get; init; }
    public required LoanStatus Status { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? ReturnedAtUtc { get; init; }
    public required IReadOnlyList<LoanItemDto> Items { get; init; }
}

public sealed class LoanItemDto
{
    public required Guid BookId { get; init; }
    public required string Title { get; init; }
    public required int Quantity { get; init; }
}
