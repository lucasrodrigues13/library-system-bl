using LibrarySystem.Domain.Common;

namespace LibrarySystem.Application.Abstractions;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<Result<T>> ExecuteInTransactionAsync<T>(
        Func<Task<Result<T>>> action,
        CancellationToken cancellationToken = default);
}
