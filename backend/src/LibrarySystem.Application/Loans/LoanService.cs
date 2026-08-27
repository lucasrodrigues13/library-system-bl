using LibrarySystem.Application.Abstractions;
using LibrarySystem.Domain.Common;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Enums;
using LibrarySystem.Domain.Loans;

namespace LibrarySystem.Application.Loans;

public sealed class LoanService : ILoanService
{
    private readonly ILoanRepository _loans;
    private readonly IBookRepository _books;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public LoanService(
        ILoanRepository loans,
        IBookRepository books,
        IUserRepository users,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _loans = loans;
        _books = books;
        _users = users;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result<IReadOnlyList<LoanDto>>> ListAsync(CancellationToken cancellationToken = default)
    {
        var loans = await _loans.ListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<LoanDto>>(loans.Select(l => l.ToDto()).ToList());
    }

    public async Task<Result<LoanDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var loan = await _loans.GetByIdAsync(id, cancellationToken);
        return loan is null
            ? Result.Failure<LoanDto>(Errors.NotFound("Loan"))
            : Result.Success(loan.ToDto());
    }

    public async Task<Result<LoanDto>> CreateAsync(
        CreateLoanRequest request,
        Guid adminId,
        CancellationToken cancellationToken = default)
    {
        var shape = LoanPolicy.ValidateShape(request.BookIds);
        if (shape.IsFailure)
        {
            return Result.Failure<LoanDto>(shape.Error!);
        }

        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var borrower = await _users.GetByIdAsync(request.BorrowerId, cancellationToken);
            if (borrower is null)
            {
                return Result.Failure<LoanDto>(Errors.NotFound("User"));
            }

            if (borrower.Role != UserRole.Client)
            {
                return Result.Failure<LoanDto>(Errors.BorrowerMustBeClient());
            }

            var books = await _books.GetByIdsAsync(request.BookIds, cancellationToken);
            var booksById = books.ToDictionary(b => b.Id);
            var stock = LoanPolicy.ValidateStock(request.BookIds, booksById);
            if (stock.IsFailure)
            {
                return Result.Failure<LoanDto>(stock.Error!);
            }

            var now = _clock.UtcNow;
            var loan = new Loan
            {
                Id = Guid.NewGuid(),
                BorrowerId = borrower.Id,
                Borrower = borrower,
                CreatedByAdminId = adminId,
                Status = LoanStatus.Active,
                CreatedAtUtc = now
            };

            foreach (var bookId in request.BookIds)
            {
                var book = booksById[bookId];
                book.DecrementStock(LoanPolicy.UnitsPerTitle);
                book.UpdatedAtUtc = now;
                loan.Items.Add(new LoanItem
                {
                    Id = Guid.NewGuid(),
                    LoanId = loan.Id,
                    BookId = book.Id,
                    Book = book,
                    Quantity = LoanPolicy.UnitsPerTitle
                });
            }

            _loans.Add(loan);
            return Result.Success(loan.ToDto());
        }, cancellationToken);
    }

    public async Task<Result<LoanDto>> ReturnAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var loan = await _loans.GetByIdAsync(id, cancellationToken);
            if (loan is null)
            {
                return Result.Failure<LoanDto>(Errors.NotFound("Loan"));
            }

            var canReturn = LoanPolicy.ValidateReturn(loan);
            if (canReturn.IsFailure)
            {
                return Result.Failure<LoanDto>(canReturn.Error!);
            }

            var now = _clock.UtcNow;
            loan.MarkReturned(now);

            foreach (var item in loan.Items)
            {
                item.Book.IncrementStock(item.Quantity);
                item.Book.UpdatedAtUtc = now;
            }

            return Result.Success(loan.ToDto());
        }, cancellationToken);
    }
}
