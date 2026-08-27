namespace LibrarySystem.Domain.Entities;

public sealed class Book
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public byte[]? RowVersion { get; set; }

    public ICollection<LoanItem> LoanItems { get; set; } = new List<LoanItem>();

    public void DecrementStock(int units)
    {
        Quantity -= units;
    }

    public void IncrementStock(int units)
    {
        Quantity = Math.Min(Quantity + units, TotalQuantity);
    }
}
