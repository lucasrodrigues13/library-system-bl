using LibrarySystem.Domain.Enums;

namespace LibrarySystem.Application.Auth;

public sealed class CurrentUserDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required UserRole Role { get; init; }
}
