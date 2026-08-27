using LibrarySystem.Domain.Common;
using LibrarySystem.Domain.Entities;

namespace LibrarySystem.Application.Users;

public interface IUserService
{
    Task<Result<IReadOnlyList<UserDto>>> ListAsync(CancellationToken cancellationToken = default);
    Task<Result<UserDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default);
}

internal static class UserMappings
{
    public static UserDto ToDto(this User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        Role = user.Role,
        CreatedAtUtc = user.CreatedAtUtc
    };
}
