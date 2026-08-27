using LibrarySystem.Domain.Entities;

namespace LibrarySystem.Application.Abstractions;

public interface IPasswordHasher
{
    string Hash(User user, string password);
    bool Verify(User user, string password);
}
