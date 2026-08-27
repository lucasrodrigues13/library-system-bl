using FluentAssertions;
using LibrarySystem.Application.Abstractions;
using LibrarySystem.Application.Books;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Enums;
using NSubstitute;

namespace LibrarySystem.Application.Tests;

public class BookServiceTests
{
    [Fact]
    public async Task List_hides_zero_stock_titles_from_clients()
    {
        var books = Substitute.For<IBookRepository>();
        books.ListAsync(true).Returns([
            new Book { Id = Guid.NewGuid(), Title = "Dune", Author = "Herbert", Isbn = "1", TotalQuantity = 1, Quantity = 1 }
        ]);
        var sut = new BookService(books, Substitute.For<ILoanRepository>(), Substitute.For<IUnitOfWork>(), Substitute.For<IClock>());

        var result = await sut.ListAsync(UserRole.Client);

        result.Value.Should().ContainSingle(b => b.Title == "Dune");
        await books.Received(1).ListAsync(true);
    }

    [Fact]
    public async Task GetById_hides_zero_stock_from_clients()
    {
        var id = Guid.NewGuid();
        var books = Substitute.For<IBookRepository>();
        books.GetByIdAsync(id).Returns(new Book { Id = id, Title = "Gone", Author = "A", Isbn = "0", TotalQuantity = 0, Quantity = 0 });
        var sut = new BookService(books, Substitute.For<ILoanRepository>(), Substitute.For<IUnitOfWork>(), Substitute.For<IClock>());

        var result = await sut.GetByIdAsync(id, UserRole.Client);

        result.Error!.Code.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Create_rejects_available_greater_than_total()
    {
        var books = Substitute.For<IBookRepository>();
        var clock = Substitute.For<IClock>();
        clock.UtcNow.Returns(DateTime.UtcNow);
        var sut = new BookService(books, Substitute.For<ILoanRepository>(), Substitute.For<IUnitOfWork>(), clock);

        var result = await sut.CreateAsync(new UpsertBookRequest
        {
            Title = "Dune",
            Author = "Herbert",
            Isbn = "9780441172719",
            TotalQuantity = 1,
            Quantity = 2
        });

        result.Error!.Code.Should().Be("INVALID_INPUT");
        books.DidNotReceive().Add(Arg.Any<Book>());
    }
}
