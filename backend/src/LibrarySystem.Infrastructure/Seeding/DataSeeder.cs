using LibrarySystem.Application.Abstractions;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Enums;
using LibrarySystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Infrastructure.Seeding;

public sealed class DataSeeder
{
    public const string AdminEmail = "admin@library.local";
    public const string AdminPassword = "Admin123!";
    public const string AliceEmail = "alice@library.local";
    public const string AlicePassword = "Alice123!";
    public const string BobEmail = "bob@library.local";
    public const string BobPassword = "Bob123!";
    public const string CarolEmail = "carol@library.local";
    public const string CarolPassword = "Carol123!";

    private readonly LibraryDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IClock _clock;

    public DataSeeder(LibraryDbContext db, IPasswordHasher passwordHasher, IClock clock)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _clock = clock;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _db.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = _clock.UtcNow;

        var admin = CreateUser(SeedIds.AdminId, "Library Admin", AdminEmail, AdminPassword, UserRole.Admin, now);
        var alice = CreateUser(SeedIds.AliceId, "Alice Reader", AliceEmail, AlicePassword, UserRole.Client, now);
        var bob = CreateUser(SeedIds.BobId, "Bob Reader", BobEmail, BobPassword, UserRole.Client, now);
        var carol = CreateUser(SeedIds.CarolId, "Carol Reader", CarolEmail, CarolPassword, UserRole.Client, now);

        var hobbit = CreateBook(SeedIds.HobbitId, "The Hobbit", "J.R.R. Tolkien", "9780547928227", 5, 5, now);
        var pride = CreateBook(SeedIds.PrideId, "Pride and Prejudice", "Jane Austen", "9780141439518", 3, 3, now);
        var orwell = CreateBook(SeedIds.NineteenEightyFourId, "1984", "George Orwell", "9780451524935", 2, 2, now);
        var dune = CreateBook(SeedIds.DuneId, "Dune", "Frank Herbert", "9780441172719", 1, 1, now);
        var outOfStock = CreateBook(SeedIds.OutOfStockId, "Out of Print Tales", "Anonymous", "9780000000000", 0, 0, now);
        var cleanCode = CreateBook(SeedIds.CleanCodeId, "Clean Code", "Robert C. Martin", "9780132350884", 8, 8, now);
        var pragmatic = CreateBook(SeedIds.PragmaticId, "The Pragmatic Programmer", "Andrew Hunt", "9780201616224", 4, 4, now);
        var ddd = CreateBook(SeedIds.DddId, "Domain-Driven Design", "Eric Evans", "9780321125217", 6, 6, now);

        // Alice already borrowed The Hobbit; remaining Hobbit stock is 4 of 5 owned copies.
        hobbit.Quantity = 4;
        var loan = new Loan
        {
            Id = SeedIds.ActiveLoanId,
            BorrowerId = alice.Id,
            CreatedByAdminId = admin.Id,
            Status = LoanStatus.Active,
            CreatedAtUtc = now,
            Items =
            {
                new LoanItem
                {
                    Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc1"),
                    LoanId = SeedIds.ActiveLoanId,
                    BookId = hobbit.Id,
                    Quantity = 1
                }
            }
        };

        _db.Users.AddRange(admin, alice, bob, carol);
        _db.Books.AddRange(hobbit, pride, orwell, dune, outOfStock, cleanCode, pragmatic, ddd);
        _db.Loans.Add(loan);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private User CreateUser(Guid id, string name, string email, string password, UserRole role, DateTime now)
    {
        var user = new User
        {
            Id = id,
            Name = name,
            Email = email,
            Role = role,
            CreatedAtUtc = now
        };
        user.PasswordHash = _passwordHasher.Hash(user, password);
        return user;
    }

    private static Book CreateBook(Guid id, string title, string author, string isbn, int totalQuantity, int quantity, DateTime now) =>
        new()
        {
            Id = id,
            Title = title,
            Author = author,
            Isbn = isbn,
            TotalQuantity = totalQuantity,
            Quantity = quantity,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
}
