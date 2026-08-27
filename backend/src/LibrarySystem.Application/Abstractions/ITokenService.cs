using LibrarySystem.Domain.Entities;

namespace LibrarySystem.Application.Abstractions;

public interface ITokenService
{
    string CreateToken(User user);
}
