using LibrarySystem.Application.Abstractions;
using LibrarySystem.Domain.Common;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Enums;

namespace LibrarySystem.Application.Books;

public sealed class BookService : IBookService
{
    private readonly IBookRepository _books;
    private readonly ILoanRepository _loans;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public BookService(IBookRepository books, ILoanRepository loans, IUnitOfWork unitOfWork, IClock clock)
    {
        _books = books;
        _loans = loans;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result<IReadOnlyList<BookDto>>> ListAsync(UserRole role, CancellationToken cancellationToken = default)
    {
        var availableOnly = role == UserRole.Client;
        var books = await _books.ListAsync(availableOnly, cancellationToken);
        return Result.Success<IReadOnlyList<BookDto>>(books.Select(b => b.ToDto()).ToList());
    }

    public async Task<Result<BookDto>> GetByIdAsync(Guid id, UserRole role, CancellationToken cancellationToken = default)
    {
        var book = await _books.GetByIdAsync(id, cancellationToken);
        if (book is null)
        {
            return Result.Failure<BookDto>(Errors.NotFound("Book"));
        }

        if (role == UserRole.Client && book.Quantity <= 0)
        {
            return Result.Failure<BookDto>(Errors.NotFound("Book"));
        }

        return Result.Success(book.ToDto());
    }

    public async Task<Result<BookDto>> CreateAsync(UpsertBookRequest request, CancellationToken cancellationToken = default)
    {
        var validation = Validate(request);
        if (validation.IsFailure)
        {
            return Result.Failure<BookDto>(validation.Error!);
        }

        var isbn = request.Isbn.Trim();
        if (await _books.IsbnExistsAsync(isbn, null, cancellationToken))
        {
            return Result.Failure<BookDto>(Errors.DuplicateIsbn());
        }

        var now = _clock.UtcNow;
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Author = request.Author.Trim(),
            Isbn = isbn,
            TotalQuantity = request.TotalQuantity,
            Quantity = request.Quantity,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _books.Add(book);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(book.ToDto());
    }

    public async Task<Result<BookDto>> UpdateAsync(Guid id, UpsertBookRequest request, CancellationToken cancellationToken = default)
    {
        var book = await _books.GetByIdAsync(id, cancellationToken);
        if (book is null)
        {
            return Result.Failure<BookDto>(Errors.NotFound("Book"));
        }

        var validation = ValidateUpdate(request, book);
        if (validation.IsFailure)
        {
            return Result.Failure<BookDto>(validation.Error!);
        }

        var isbn = request.Isbn.Trim();
        if (await _books.IsbnExistsAsync(isbn, id, cancellationToken))
        {
            return Result.Failure<BookDto>(Errors.DuplicateIsbn());
        }

        book.Title = request.Title.Trim();
        book.Author = request.Author.Trim();
        book.Isbn = isbn;
        book.TotalQuantity = request.TotalQuantity;
        book.Quantity = request.Quantity;
        book.UpdatedAtUtc = _clock.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(book.ToDto());
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var book = await _books.GetByIdAsync(id, cancellationToken);
        if (book is null)
        {
            return Result.Failure(Errors.NotFound("Book"));
        }

        if (await _loans.HasActiveLoansForBookAsync(id, cancellationToken))
        {
            return Result.Failure(Errors.BookHasActiveLoans());
        }

        _books.Remove(book);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static Result Validate(UpsertBookRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Result.Failure(Errors.InvalidInput("Title is required."));
        }

        if (string.IsNullOrWhiteSpace(request.Author))
        {
            return Result.Failure(Errors.InvalidInput("Author is required."));
        }

        if (string.IsNullOrWhiteSpace(request.Isbn))
        {
            return Result.Failure(Errors.InvalidInput("ISBN is required."));
        }

        if (request.TotalQuantity < 0)
        {
            return Result.Failure(Errors.InvalidInput("Total units cannot be negative."));
        }

        if (request.Quantity < 0)
        {
            return Result.Failure(Errors.InvalidInput("Available copies cannot be negative."));
        }

        if (request.Quantity > request.TotalQuantity)
        {
            return Result.Failure(Errors.InvalidInput("Available copies cannot exceed total units."));
        }

        return Result.Success();
    }

    private static Result ValidateUpdate(UpsertBookRequest request, Book book)
    {
        var baseValidation = Validate(request);
        if (baseValidation.IsFailure)
        {
            return baseValidation;
        }

        var loaned = Math.Max(0, book.TotalQuantity - book.Quantity);
        if (request.TotalQuantity < loaned)
        {
            return Result.Failure(Errors.InvalidInput("Total units cannot be less than copies currently on loan."));
        }

        if (request.Quantity > request.TotalQuantity - loaned)
        {
            return Result.Failure(Errors.InvalidInput("Available copies cannot exceed total units minus copies on loan."));
        }

        return Result.Success();
    }
}
