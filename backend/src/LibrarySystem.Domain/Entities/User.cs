using LibrarySystem.Domain.Enums;

namespace LibrarySystem.Domain.Entities;

public sealed class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public ICollection<Loan> LoansAsBorrower { get; set; } = new List<Loan>();
}
