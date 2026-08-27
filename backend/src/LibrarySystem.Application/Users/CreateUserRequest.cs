using LibrarySystem.Domain.Enums;

namespace LibrarySystem.Application.Users;

public sealed class CreateUserRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Client;
}
