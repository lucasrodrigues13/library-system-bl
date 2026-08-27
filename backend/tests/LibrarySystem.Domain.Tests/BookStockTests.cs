using FluentAssertions;
using LibrarySystem.Domain.Entities;

namespace LibrarySystem.Domain.Tests;

public class BookStockTests
{
    [Fact]
    public void IncrementStock_does_not_exceed_total_units()
    {
        var book = new Book { Title = "Dune", TotalQuantity = 2, Quantity = 1 };
        book.IncrementStock(1);
        book.Quantity.Should().Be(2);
        book.IncrementStock(1);
        book.Quantity.Should().Be(2);
        book.TotalQuantity.Should().Be(2);
    }

    [Fact]
    public void DecrementStock_reduces_available_copies_only()
    {
        var book = new Book { Title = "Dune", TotalQuantity = 3, Quantity = 3 };
        book.DecrementStock(1);
        book.Quantity.Should().Be(2);
        book.TotalQuantity.Should().Be(3);
    }
}
