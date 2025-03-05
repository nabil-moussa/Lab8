using Lab8.Auth.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lab8.Auth.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection") ?? 
                "Host=localhost;Database=AuthDb;Username=nabilmoussa123;Password=mysecretpassword"
            )
        );

        return services;
    }
}