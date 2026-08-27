using LibrarySystem.Domain.Common;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Enums;

namespace LibrarySystem.Application.Books;

public interface IBookService
{
    Task<Result<IReadOnlyList<BookDto>>> ListAsync(UserRole role, CancellationToken cancellationToken = default);
    Task<Result<BookDto>> GetByIdAsync(Guid id, UserRole role, CancellationToken cancellationToken = default);
    Task<Result<BookDto>> CreateAsync(UpsertBookRequest request, CancellationToken cancellationToken = default);
    Task<Result<BookDto>> UpdateAsync(Guid id, UpsertBookRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

internal static class BookMappings
{
    public static BookDto ToDto(this Book book) => new()
    {
        Id = book.Id,
        Title = book.Title,
        Author = book.Author,
        Isbn = book.Isbn,
        TotalQuantity = book.TotalQuantity,
        Quantity = book.Quantity
    };
}
