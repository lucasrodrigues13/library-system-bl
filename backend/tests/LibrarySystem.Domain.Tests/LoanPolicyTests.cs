using FluentAssertions;
using LibrarySystem.Domain.Common;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Enums;
using LibrarySystem.Domain.Loans;

namespace LibrarySystem.Domain.Tests;

public class LoanPolicyTests
{
    private static Book Book(Guid id, string title, int quantity) => new()
    {
        Id = id,
        Title = title,
        Author = "Author",
        Isbn = id.ToString("N")[..13],
        Quantity = quantity,
        TotalQuantity = Math.Max(quantity, 1)
    };

    [Fact]
    public void ValidateCreate_rejects_empty_selection()
    {
        var result = LoanPolicy.ValidateCreate([], new Dictionary<Guid, Book>());
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("LOAN_EMPTY");
    }

    [Fact]
    public void ValidateCreate_rejects_more_than_three_titles()
    {
        var ids = Enumerable.Range(0, 4).Select(_ => Guid.NewGuid()).ToList();
        var books = ids.ToDictionary(id => id, id => Book(id, "T", 1));

        var result = LoanPolicy.ValidateCreate(ids, books);

        result.Error!.Code.Should().Be("LOAN_TOO_MANY_TITLES");
    }

    [Fact]
    public void ValidateCreate_rejects_duplicate_titles()
    {
        var id = Guid.NewGuid();
        var result = LoanPolicy.ValidateCreate([id, id], new Dictionary<Guid, Book> { [id] = Book(id, "Dune", 5) });
        result.Error!.Code.Should().Be("LOAN_DUPLICATE_TITLE");
    }

    [Fact]
    public void ValidateCreate_rejects_missing_book()
    {
        var missing = Guid.NewGuid();
        var result = LoanPolicy.ValidateCreate([missing], new Dictionary<Guid, Book>());
        result.Error!.Code.Should().Be("NOT_FOUND");
    }

    [Fact]
    public void ValidateCreate_fails_atomically_when_any_title_is_out_of_stock()
    {
        var available = Guid.NewGuid();
        var unavailable = Guid.NewGuid();
        var books = new Dictionary<Guid, Book>
        {
            [available] = Book(available, "The Hobbit", 4),
            [unavailable] = Book(unavailable, "Out of Print Tales", 0)
        };

        var result = LoanPolicy.ValidateCreate([available, unavailable], books);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("INSUFFICIENT_STOCK");
        result.Error.Details.Should().ContainSingle(d => d.Title == "Out of Print Tales" && d.Available == 0);
        result.Error.Details.Should().NotContain(d => d.BookId == available);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void ValidateCreate_accepts_one_to_three_available_titles(int count)
    {
        var ids = Enumerable.Range(0, count).Select(_ => Guid.NewGuid()).ToList();
        var books = ids.ToDictionary(id => id, id => Book(id, "Title", 1));

        LoanPolicy.ValidateCreate(ids, books).IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateReturn_rejects_already_returned_loan()
    {
        var loan = new Loan { Status = LoanStatus.Returned };
        var result = LoanPolicy.ValidateReturn(loan);
        result.Error!.Code.Should().Be("LOAN_ALREADY_RETURNED");
    }

    [Fact]
    public void ValidateReturn_accepts_active_loan()
    {
        LoanPolicy.ValidateReturn(new Loan { Status = LoanStatus.Active }).IsSuccess.Should().BeTrue();
    }
}
