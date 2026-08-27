using LibrarySystem.Application.Auth;
using LibrarySystem.Application.Books;
using LibrarySystem.Application.Loans;
using LibrarySystem.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace LibrarySystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<ILoanService, LoanService>();
        return services;
    }
}
