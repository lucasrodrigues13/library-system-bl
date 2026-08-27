using FluentAssertions;
using LibrarySystem.Application.Loans;
using LibrarySystem.Domain.Enums;
using LibrarySystem.Infrastructure.Persistence;
using LibrarySystem.Infrastructure.Persistence.Repositories;
using LibrarySystem.Infrastructure.Seeding;
using LibrarySystem.Infrastructure.Security;
using LibrarySystem.Infrastructure.Time;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Infrastructure.Tests;

public class DataSeederTests
{
    [Fact]
    public async Task Seed_is_idempotent()
    {
        await using var fixture = await SqliteFixture.CreateAsync();
        var seeder = new DataSeeder(fixture.Db, new PasswordHasherAdapter(), new SystemClock());

        await seeder.SeedAsync();
        await seeder.SeedAsync();

        (await fixture.Db.Users.CountAsync()).Should().Be(4);
        (await fixture.Db.Books.CountAsync()).Should().Be(8);
        (await fixture.Db.Loans.CountAsync()).Should().Be(1);
        var hobbit = await fixture.Db.Books.SingleAsync(b => b.Id == SeedIds.HobbitId);
        hobbit.TotalQuantity.Should().Be(5);
        hobbit.Quantity.Should().Be(4);
    }
}

public class LoanTransactionTests
{
    [Fact]
    public async Task Create_does_not_commit_partial_items_when_stock_is_insufficient()
    {
        await using var fixture = await SqliteFixture.CreateAsync();
        var hasher = new PasswordHasherAdapter();
        var clock = new SystemClock();
        await new DataSeeder(fixture.Db, hasher, clock).SeedAsync();

        var service = new LoanService(
            new LoanRepository(fixture.Db),
            new BookRepository(fixture.Db),
            new UserRepository(fixture.Db),
            new UnitOfWork(fixture.Db),
            clock);

        var duneQty = await fixture.Db.Books.Where(b => b.Id == SeedIds.DuneId).Select(b => b.Quantity).SingleAsync();
        duneQty.Should().Be(1);

        var result = await service.CreateAsync(
            new CreateLoanRequest
            {
                BorrowerId = SeedIds.BobId,
                BookIds = [SeedIds.DuneId, SeedIds.OutOfStockId]
            },
            SeedIds.AdminId);

        result.Error!.Code.Should().Be("INSUFFICIENT_STOCK");
        (await fixture.Db.Loans.CountAsync()).Should().Be(1);
        (await fixture.Db.Books.Where(b => b.Id == SeedIds.DuneId).Select(b => b.Quantity).SingleAsync()).Should().Be(1);
    }
}

internal sealed class SqliteFixture : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    public LibraryDbContext Db { get; }

    private SqliteFixture(SqliteConnection connection, LibraryDbContext db)
    {
        _connection = connection;
        Db = db;
    }

    public static async Task<SqliteFixture> CreateAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<LibraryDbContext>().UseSqlite(connection).Options;
        var db = new LibraryDbContext(options);
        await db.Database.EnsureCreatedAsync();
        return new SqliteFixture(connection, db);
    }

    public async ValueTask DisposeAsync()
    {
        await Db.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
