using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelPlaner.Application.Interfaces;
using TravelPlaner.Infrastructure.Auth;
using TravelPlaner.Infrastructure.Persistence;
using TravelPlaner.Infrastructure.Repositories;
using TravelPlaner.Infrastructure.Storage;

namespace TravelPlaner.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var persistenceProvider = configuration["PERSISTENCE_PROVIDER"] ?? "sqlite";

        services.AddDbContext<AppDbContext>(options =>
        {
            if (persistenceProvider.Equals("postgres", StringComparison.OrdinalIgnoreCase))
            {
                var connStr = configuration["CONNECTIONSTRINGS__POSTGRES"]
                    ?? configuration.GetConnectionString("Postgres")
                    ?? throw new InvalidOperationException("Postgres connection string not configured.");
                options.UseNpgsql(connStr);
            }
            else
            {
                var dbPath = configuration["SQLITE__PATH"] ?? "/data/travelplanner.db";
                var dir = Path.GetDirectoryName(dbPath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                options.UseSqlite($"Data Source={dbPath}");
            }
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<IJourneyRepository, JourneyRepository>();

        services.Configure<ImageStoreOptions>(opts =>
        {
            opts.BasePath = Path.GetFullPath(configuration["IMAGE_STORE_PATH"] ?? "/data/images");
            opts.BaseUrl = "/images";
        });
        services.AddSingleton<IImageStore, FileSystemImageStore>();

        services.Configure<JwtOptions>(opts =>
        {
            opts.SigningKey = configuration["AUTH_LOCAL_JWT_SIGNING_KEY"]
                ?? configuration[$"{JwtOptions.Section}:{nameof(JwtOptions.SigningKey)}"]
                ?? throw new InvalidOperationException("JWT signing key not configured.");
            opts.Issuer = configuration[$"{JwtOptions.Section}:{nameof(JwtOptions.Issuer)}"] ?? "travelplaner";
            opts.Audience = configuration[$"{JwtOptions.Section}:{nameof(JwtOptions.Audience)}"] ?? "travelplaner";
        });
        services.AddScoped<IAuthService, LocalAuthService>();

        return services;
    }

    public static async Task ApplyMigrationsAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }
}
