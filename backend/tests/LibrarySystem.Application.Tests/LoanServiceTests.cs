using FluentAssertions;
using LibrarySystem.Application.Abstractions;
using LibrarySystem.Application.Loans;
using LibrarySystem.Domain.Common;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Enums;
using NSubstitute;

namespace LibrarySystem.Application.Tests;

public class LoanServiceTests
{
    private readonly ILoanRepository _loans = Substitute.For<ILoanRepository>();
    private readonly IBookRepository _books = Substitute.For<IBookRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly LoanService _sut;

    public LoanServiceTests()
    {
        _clock.UtcNow.Returns(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        _uow.ExecuteInTransactionAsync(Arg.Any<Func<Task<Result<LoanDto>>>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Func<Task<Result<LoanDto>>>>().Invoke());
        _sut = new LoanService(_loans, _books, _users, _uow, _clock);
    }

    [Fact]
    public async Task Create_rejects_admin_borrower()
    {
        var adminId = Guid.NewGuid();
        _users.GetByIdAsync(adminId).Returns(new User { Id = adminId, Role = UserRole.Admin, Name = "Admin" });

        var result = await _sut.CreateAsync(
            new CreateLoanRequest { BorrowerId = adminId, BookIds = [Guid.NewGuid()] },
            Guid.NewGuid());

        result.Error!.Code.Should().Be("INVALID_INPUT");
    }

    [Fact]
    public async Task Create_maps_insufficient_stock_to_error_code()
    {
        var borrowerId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        _users.GetByIdAsync(borrowerId).Returns(new User { Id = borrowerId, Role = UserRole.Client, Name = "Alice" });
        _books.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>()).Returns([
            new Book { Id = bookId, Title = "Dune", Quantity = 0, TotalQuantity = 1, Author = "Herbert", Isbn = "1" }
        ]);

        var result = await _sut.CreateAsync(
            new CreateLoanRequest { BorrowerId = borrowerId, BookIds = [bookId] },
            Guid.NewGuid());

        result.Error!.Code.Should().Be("INSUFFICIENT_STOCK");
        result.Error.Details.Should().ContainSingle(d => d.Title == "Dune");
        _loans.DidNotReceive().Add(Arg.Any<Loan>());
    }

    [Fact]
    public async Task Create_succeeds_and_decrements_stock()
    {
        var borrowerId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var book = new Book { Id = bookId, Title = "Dune", Quantity = 1, TotalQuantity = 1, Author = "Herbert", Isbn = "1" };
        _users.GetByIdAsync(borrowerId).Returns(new User { Id = borrowerId, Role = UserRole.Client, Name = "Alice" });
        _books.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>()).Returns([book]);

        var result = await _sut.CreateAsync(
            new CreateLoanRequest { BorrowerId = borrowerId, BookIds = [bookId] },
            Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        book.Quantity.Should().Be(0);
        _loans.Received(1).Add(Arg.Any<Loan>());
    }
}
