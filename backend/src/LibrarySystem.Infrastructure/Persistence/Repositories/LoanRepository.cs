using LibrarySystem.Application.Abstractions;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Infrastructure.Persistence.Repositories;

public sealed class LoanRepository : ILoanRepository
{
    private readonly LibraryDbContext _db;

    public LoanRepository(LibraryDbContext db)
    {
        _db = db;
    }

    public Task<Loan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Loans
            .Include(x => x.Borrower)
            .Include(x => x.Items)
            .ThenInclude(x => x.Book)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Loan>> ListAsync(CancellationToken cancellationToken = default) =>
        await _db.Loans
            .AsNoTracking()
            .Include(x => x.Borrower)
            .Include(x => x.Items)
            .ThenInclude(x => x.Book)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public Task<bool> HasActiveLoansForUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _db.Loans.AnyAsync(x => x.BorrowerId == userId && x.Status == LoanStatus.Active, cancellationToken);

    public Task<bool> HasActiveLoansForBookAsync(Guid bookId, CancellationToken cancellationToken = default) =>
        _db.LoanItems.AnyAsync(
            x => x.BookId == bookId && x.Loan.Status == LoanStatus.Active,
            cancellationToken);

    public void Add(Loan loan) => _db.Loans.Add(loan);
}
