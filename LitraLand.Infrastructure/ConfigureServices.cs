using LitraLand.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace LitraLand.Infrastructure;

/// <summary>
/// Provides an extension method to configure and register infrastructure-related services.
/// </summary>
public static class ConfigureServices
{
    /// <summary>
    /// Registers infrastructure services, including the database context and related dependencies.
    /// </summary>
    /// <param name="services">The service collection to which dependencies will be added.</param>
    /// <param name="configuration">The application configuration that provides necessary settings.</param>
    /// <returns>
    /// The modified <see cref="IServiceCollection"/> containing the registered infrastructure services.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the required database connection string is not found in the configuration.
    /// </exception>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Retrieve the database connection string from configuration
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // Register the application's DbContext with SQL Server support
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName) // Specify the assembly containing the migrations
            ));

        // remove the above line and uncomment the below lines if you want to use the options for SQL Server (CommandTimeout, EnableRetryOnFailure)
        //services.AddDbContext<ApplicationDbContext>(options =>
        //    options.UseSqlServer(
        //        connectionString,
        //        sqlOptions =>
        //        {
        //            // Specify the assembly containing the migrations
        //            sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);

        //            // Set command timeout to 60 seconds
        //            sqlOptions.CommandTimeout(60);

        //            // Enable retry on failure (3 retries, wait 5 seconds between retries)
        //            sqlOptions.EnableRetryOnFailure(
        //                maxRetryCount: 3,
        //                maxRetryDelay: TimeSpan.FromSeconds(5),
        //                errorNumbersToAdd: null
        //            );
        //        }));

        // Register the DbContext interface for dependency injection
        services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

        return services;
    }
}