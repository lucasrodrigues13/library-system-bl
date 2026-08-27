using LibrarySystem.Infrastructure.Persistence;
using LibrarySystem.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LibrarySystem.Infrastructure;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<LibraryDbContext>>();
        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();

        await WaitForDatabaseAsync(db, logger, cancellationToken);

        if (db.Database.GetMigrations().Any())
        {
            await db.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await db.Database.EnsureCreatedAsync(cancellationToken);
        }

        await seeder.SeedAsync(cancellationToken);
    }

    private static async Task WaitForDatabaseAsync(
        LibraryDbContext db,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        const int maxAttempts = 20;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                if (await db.Database.CanConnectAsync(cancellationToken))
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Database not ready (attempt {Attempt}/{Max}).", attempt, maxAttempts);
            }

            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }
    }
}
