using LibrarySystem.Domain.Enums;

namespace LibrarySystem.Application.Users;

public sealed class UpdateUserRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Password { get; set; }
    public UserRole Role { get; set; }
}
