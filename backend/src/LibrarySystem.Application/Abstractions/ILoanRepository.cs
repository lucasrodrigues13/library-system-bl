using LibrarySystem.Domain.Entities;

namespace LibrarySystem.Application.Abstractions;

public interface ILoanRepository
{
    Task<Loan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Loan>> ListAsync(CancellationToken cancellationToken = default);
    Task<bool> HasActiveLoansForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> HasActiveLoansForBookAsync(Guid bookId, CancellationToken cancellationToken = default);
    void Add(Loan loan);
}
