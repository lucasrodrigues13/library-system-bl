using FluentAssertions;
using LibrarySystem.Application.Abstractions;
using LibrarySystem.Application.Auth;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Enums;
using NSubstitute;

namespace LibrarySystem.Application.Tests;

public class AuthServiceTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenService _tokens = Substitute.For<ITokenService>();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(_users, _hasher, _tokens);
    }

    [Fact]
    public async Task Login_returns_token_when_credentials_match()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            Email = "admin@library.local",
            Role = UserRole.Admin
        };
        _users.GetByEmailAsync("admin@library.local").Returns(user);
        _hasher.Verify(user, "Admin123!").Returns(true);
        _tokens.CreateToken(user).Returns("token");

        var result = await _sut.LoginAsync(new LoginRequest { Email = "admin@library.local", Password = "Admin123!" });

        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().Be("token");
        result.Value.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public async Task Login_fails_for_unknown_user()
    {
        _users.GetByEmailAsync("nobody@library.local").Returns((User?)null);

        var result = await _sut.LoginAsync(new LoginRequest { Email = "nobody@library.local", Password = "secret12" });

        result.Error!.Code.Should().Be("INVALID_CREDENTIALS");
    }
}
