using LibrarySystem.Domain.Enums;

namespace LibrarySystem.Application.Users;

public sealed class UserDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required UserRole Role { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
}
