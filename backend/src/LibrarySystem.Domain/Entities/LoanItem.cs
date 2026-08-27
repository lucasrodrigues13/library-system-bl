namespace LibrarySystem.Domain.Entities;

public sealed class LoanItem
{
    public Guid Id { get; set; }
    public Guid LoanId { get; set; }
    public Guid BookId { get; set; }
    public int Quantity { get; set; } = 1;

    public Loan Loan { get; set; } = null!;
    public Book Book { get; set; } = null!;
}
