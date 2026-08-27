using LibrarySystem.Domain.Entities;

namespace LibrarySystem.Application.Abstractions;

public interface IBookRepository
{
    Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Book>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Book>> ListAsync(bool availableOnly, CancellationToken cancellationToken = default);
    Task<bool> IsbnExistsAsync(string isbn, Guid? excludingBookId = null, CancellationToken cancellationToken = default);
    void Add(Book book);
    void Remove(Book book);
}
