using LibrarySystem.Domain.Common;
using LibrarySystem.Domain.Entities;

namespace LibrarySystem.Application.Loans;

public interface ILoanService
{
    Task<Result<IReadOnlyList<LoanDto>>> ListAsync(CancellationToken cancellationToken = default);
    Task<Result<LoanDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<LoanDto>> CreateAsync(CreateLoanRequest request, Guid adminId, CancellationToken cancellationToken = default);
    Task<Result<LoanDto>> ReturnAsync(Guid id, CancellationToken cancellationToken = default);
}

internal static class LoanMappings
{
    public static LoanDto ToDto(this Loan loan) => new()
    {
        Id = loan.Id,
        BorrowerId = loan.BorrowerId,
        BorrowerName = loan.Borrower?.Name ?? string.Empty,
        CreatedByAdminId = loan.CreatedByAdminId,
        Status = loan.Status,
        CreatedAtUtc = loan.CreatedAtUtc,
        ReturnedAtUtc = loan.ReturnedAtUtc,
        Items = loan.Items.Select(item => new LoanItemDto
        {
            BookId = item.BookId,
            Title = item.Book?.Title ?? string.Empty,
            Quantity = item.Quantity
        }).ToList()
    };
}
