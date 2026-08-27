namespace LibrarySystem.Application.Loans;

public sealed class CreateLoanRequest
{
    public Guid BorrowerId { get; set; }
    public List<Guid> BookIds { get; set; } = [];
}
