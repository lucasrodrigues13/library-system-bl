using LibrarySystem.Application.Abstractions;
using LibrarySystem.Domain.Common;

namespace LibrarySystem.Application.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokens;

    public AuthService(IUserRepository users, IPasswordHasher passwordHasher, ITokenService tokens)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokens = tokens;
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Result.Failure<LoginResponse>(Errors.InvalidCredentials());
        }

        var user = await _users.GetByEmailAsync(request.Email.Trim(), cancellationToken);
        if (user is null || !_passwordHasher.Verify(user, request.Password))
        {
            return Result.Failure<LoginResponse>(Errors.InvalidCredentials());
        }

        return Result.Success(new LoginResponse
        {
            Token = _tokens.CreateToken(user),
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role
        });
    }

    public async Task<Result<CurrentUserDto>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<CurrentUserDto>(Errors.NotFound("User"));
        }

        return Result.Success(new CurrentUserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role
        });
    }
}
