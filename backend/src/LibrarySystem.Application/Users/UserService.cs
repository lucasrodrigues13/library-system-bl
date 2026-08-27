using LibrarySystem.Application.Abstractions;
using LibrarySystem.Domain.Common;
using LibrarySystem.Domain.Entities;

namespace LibrarySystem.Application.Users;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly ILoanRepository _loans;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public UserService(
        IUserRepository users,
        ILoanRepository loans,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _users = users;
        _loans = loans;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result<IReadOnlyList<UserDto>>> ListAsync(CancellationToken cancellationToken = default)
    {
        var users = await _users.ListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<UserDto>>(users.Select(u => u.ToDto()).ToList());
    }

    public async Task<Result<UserDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(id, cancellationToken);
        return user is null
            ? Result.Failure<UserDto>(Errors.NotFound("User"))
            : Result.Success(user.ToDto());
    }

    public async Task<Result<UserDto>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var validation = Validate(request.Name, request.Email, request.Password, passwordRequired: true);
        if (validation.IsFailure)
        {
            return Result.Failure<UserDto>(validation.Error!);
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _users.EmailExistsAsync(email, null, cancellationToken))
        {
            return Result.Failure<UserDto>(Errors.DuplicateEmail());
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = email,
            Role = request.Role,
            CreatedAtUtc = _clock.UtcNow
        };
        user.PasswordHash = _passwordHasher.Hash(user, request.Password);

        _users.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(user.ToDto());
    }

    public async Task<Result<UserDto>> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var passwordRequired = !string.IsNullOrWhiteSpace(request.Password);
        var validation = Validate(request.Name, request.Email, request.Password, passwordRequired);
        if (validation.IsFailure)
        {
            return Result.Failure<UserDto>(validation.Error!);
        }

        var user = await _users.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserDto>(Errors.NotFound("User"));
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _users.EmailExistsAsync(email, id, cancellationToken))
        {
            return Result.Failure<UserDto>(Errors.DuplicateEmail());
        }

        user.Name = request.Name.Trim();
        user.Email = email;
        user.Role = request.Role;
        if (passwordRequired)
        {
            user.PasswordHash = _passwordHasher.Hash(user, request.Password!);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(user.ToDto());
    }

    public async Task<Result> DeleteAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        if (id == currentUserId)
        {
            return Result.Failure(Errors.CannotDeleteSelf());
        }

        var user = await _users.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return Result.Failure(Errors.NotFound("User"));
        }

        if (await _loans.HasActiveLoansForUserAsync(id, cancellationToken))
        {
            return Result.Failure(Errors.UserHasActiveLoans());
        }

        _users.Remove(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static Result Validate(string name, string email, string? password, bool passwordRequired)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(Errors.InvalidInput("Name is required."));
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            return Result.Failure(Errors.InvalidInput("A valid email is required."));
        }

        if (passwordRequired && (string.IsNullOrWhiteSpace(password) || password.Length < 8))
        {
            return Result.Failure(Errors.InvalidInput("Password must be at least 8 characters."));
        }

        return Result.Success();
    }
}
