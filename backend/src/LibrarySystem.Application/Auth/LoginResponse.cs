using LibrarySystem.Domain.Enums;

namespace LibrarySystem.Application.Auth;

public sealed class LoginResponse
{
    public required string Token { get; init; }
    public required Guid UserId { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required UserRole Role { get; init; }
}
