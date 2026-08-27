namespace LibrarySystem.Application.Books;

public sealed class BookDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Author { get; init; }
    public required string Isbn { get; init; }
    public required int TotalQuantity { get; init; }
    public required int Quantity { get; init; }
}
