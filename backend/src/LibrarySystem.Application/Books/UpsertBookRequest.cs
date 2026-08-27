namespace LibrarySystem.Application.Books;

public sealed class UpsertBookRequest
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public int Quantity { get; set; }
}
