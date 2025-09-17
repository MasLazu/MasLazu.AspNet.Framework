using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MasLazu.AspNet.Framework.EfCore.Data;

/// <summary>
/// Generic design-time factory for creating DbContext instances during EF Core design-time operations (migrations).
/// This factory handles configuration loading and connection string resolution.
/// Database provider configuration should be implemented in derived classes.
/// </summary>
/// <typeparam name="TDbContext">The type of DbContext to create</typeparam>
public abstract class DesignTimeDbContextFactory<TDbContext> : IDesignTimeDbContextFactory<TDbContext>
    where TDbContext : DbContext
{
    /// <summary>
    /// Creates a new DbContext instance for design-time operations.
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>A new DbContext instance</returns>
    public TDbContext CreateDbContext(string[] args)
    {
        IConfiguration configuration = BuildConfiguration();
        string connectionString = GetConnectionString(configuration);

        var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
        ConfigureOptions(optionsBuilder, connectionString);

        return CreateDbContextInstance(optionsBuilder.Options);
    }

    /// <summary>
    /// Builds the configuration for design-time operations.
    /// </summary>
    /// <returns>The configuration instance</returns>
    protected virtual IConfiguration BuildConfiguration()
    {
        IConfigurationBuilder builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
            .AddEnvironmentVariables();

        return builder.Build();
    }

    /// <summary>
    /// Gets the connection string from configuration.
    /// </summary>
    /// <param name="configuration">The configuration instance</param>
    /// <returns>The connection string</returns>
    protected virtual string GetConnectionString(IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("DesignConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DesignConnection' not found. " +
                "Please configure 'ConnectionStrings:DesignConnection' in appsettings.json");
        }

        return connectionString;
    }

    /// <summary>
    /// Configures the DbContext options. Override this method in derived classes to configure specific database providers.
    /// </summary>
    /// <param name="optionsBuilder">The options builder</param>
    /// <param name="connectionString">The connection string</param>
    protected abstract void ConfigureOptions(DbContextOptionsBuilder<TDbContext> optionsBuilder, string connectionString);

    /// <summary>
    /// Creates the DbContext instance with the configured options.
    /// </summary>
    /// <param name="options">The configured options</param>
    /// <returns>A new DbContext instance</returns>
    protected virtual TDbContext CreateDbContextInstance(DbContextOptions<TDbContext> options)
    {
        return (TDbContext)Activator.CreateInstance(typeof(TDbContext), options)!;
    }
}
