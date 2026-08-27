using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using LibrarySystem.Application.Auth;
using LibrarySystem.Application.Books;
using LibrarySystem.Application.Loans;
using LibrarySystem.Infrastructure.Persistence;
using LibrarySystem.Infrastructure.Seeding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LibrarySystem.Api.Tests;

public sealed class LibraryApiFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            services.AddDbContext<LibraryDbContext>(options => options.UseSqlite(_connection));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        db.Database.EnsureCreated();
        scope.ServiceProvider.GetRequiredService<DataSeeder>().SeedAsync().GetAwaiter().GetResult();
        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Dispose();
    }
}

public class ApiContractTests : IClassFixture<LibraryApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
    private readonly HttpClient _client;

    public ApiContractTests(LibraryApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_succeeds_for_seeded_admin()
    {
        var response = await LoginAsync("admin@library.local", "Admin123!");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions);
        body!.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_fails_for_bad_password()
    {
        var response = await LoginAsync("admin@library.local", "wrong-password");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var payload = await ReadError(response);
        payload.GetProperty("error").GetProperty("code").GetString().Should().Be("INVALID_CREDENTIALS");
    }

    [Fact]
    public async Task Authorized_endpoint_rejects_anonymous_calls()
    {
        var response = await _client.GetAsync("/api/v1/books");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Client_cannot_list_users()
    {
        await AuthenticateAsync("alice@library.local", "Alice123!");
        var response = await _client.GetAsync("/api/v1/users");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Client_book_list_hides_zero_stock()
    {
        await AuthenticateAsync("alice@library.local", "Alice123!");
        var response = await _client.GetAsync("/api/v1/books");
        response.EnsureSuccessStatusCode();
        var books = await response.Content.ReadFromJsonAsync<List<BookDto>>(JsonOptions);
        books.Should().NotContain(b => b.Quantity <= 0);
        books.Should().NotContain(b => b.Title == "Out of Print Tales");
    }

    [Fact]
    public async Task Admin_loan_create_returns_insufficient_stock_without_persisting()
    {
        await AuthenticateAsync("admin@library.local", "Admin123!");
        var response = await _client.PostAsJsonAsync("/api/v1/loans", new
        {
            borrowerId = SeedIds.BobId,
            bookIds = new[] { SeedIds.DuneId, SeedIds.OutOfStockId }
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var payload = await ReadError(response);
        payload.GetProperty("error").GetProperty("code").GetString().Should().Be("INSUFFICIENT_STOCK");

        var books = await _client.GetFromJsonAsync<List<BookDto>>("/api/v1/books", JsonOptions);
        books!.Single(b => b.Id == SeedIds.DuneId).Quantity.Should().Be(1);
    }

    [Fact]
    public async Task Successful_loan_decrements_quantity_and_return_restores_it()
    {
        await AuthenticateAsync("admin@library.local", "Admin123!");
        var before = (await _client.GetFromJsonAsync<List<BookDto>>("/api/v1/books", JsonOptions))!
            .Single(b => b.Id == SeedIds.PrideId).Quantity;

        var create = await _client.PostAsJsonAsync("/api/v1/loans", new
        {
            borrowerId = SeedIds.BobId,
            bookIds = new[] { SeedIds.PrideId }
        });
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var loan = await create.Content.ReadFromJsonAsync<LoanDto>(JsonOptions);

        var afterCreate = (await _client.GetFromJsonAsync<List<BookDto>>("/api/v1/books", JsonOptions))!
            .Single(b => b.Id == SeedIds.PrideId).Quantity;
        afterCreate.Should().Be(before - 1);

        var returned = await _client.PostAsync($"/api/v1/loans/{loan!.Id}/return", null);
        returned.EnsureSuccessStatusCode();

        var afterReturn = (await _client.GetFromJsonAsync<List<BookDto>>("/api/v1/books", JsonOptions))!
            .Single(b => b.Id == SeedIds.PrideId).Quantity;
        afterReturn.Should().Be(before);
    }

    private async Task AuthenticateAsync(string email, string password)
    {
        var login = await LoginAsync(email, password);
        login.EnsureSuccessStatusCode();
        var body = await login.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.Token);
    }

    private Task<HttpResponseMessage> LoginAsync(string email, string password) =>
        _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });

    private static async Task<JsonElement> ReadError(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<JsonElement>(json);
    }
}
