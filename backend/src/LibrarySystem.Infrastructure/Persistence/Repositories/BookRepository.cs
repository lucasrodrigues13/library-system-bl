using LibrarySystem.Application.Abstractions;
using LibrarySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Infrastructure.Persistence.Repositories;

public sealed class BookRepository : IBookRepository
{
    private readonly LibraryDbContext _db;

    public BookRepository(LibraryDbContext db)
    {
        _db = db;
    }

    public Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Books.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Book>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        return await _db.Books.Where(x => idList.Contains(x.Id)).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Book>> ListAsync(bool availableOnly, CancellationToken cancellationToken = default)
    {
        var query = _db.Books.AsNoTracking().AsQueryable();
        if (availableOnly)
        {
            query = query.Where(x => x.Quantity > 0);
        }

        return await query.OrderBy(x => x.Title).ToListAsync(cancellationToken);
    }

    public Task<bool> IsbnExistsAsync(string isbn, Guid? excludingBookId = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Books.Where(x => x.Isbn == isbn);
        if (excludingBookId is not null)
        {
            query = query.Where(x => x.Id != excludingBookId);
        }

        return query.AnyAsync(cancellationToken);
    }

    public void Add(Book book) => _db.Books.Add(book);

    public void Remove(Book book) => _db.Books.Remove(book);
}
