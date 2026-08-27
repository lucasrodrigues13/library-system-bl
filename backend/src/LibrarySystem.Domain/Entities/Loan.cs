using LibrarySystem.Domain.Enums;

namespace LibrarySystem.Domain.Entities;

public sealed class Loan
{
    public Guid Id { get; set; }
    public Guid BorrowerId { get; set; }
    public Guid CreatedByAdminId { get; set; }
    public LoanStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ReturnedAtUtc { get; set; }

    public User Borrower { get; set; } = null!;
    public User CreatedByAdmin { get; set; } = null!;
    public ICollection<LoanItem> Items { get; set; } = new List<LoanItem>();

    public void MarkReturned(DateTime returnedAtUtc)
    {
        Status = LoanStatus.Returned;
        ReturnedAtUtc = returnedAtUtc;
    }
}
