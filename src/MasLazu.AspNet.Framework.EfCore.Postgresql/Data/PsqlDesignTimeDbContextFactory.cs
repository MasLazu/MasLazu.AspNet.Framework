using System.Reflection;
using Microsoft.EntityFrameworkCore;
using MasLazu.AspNet.Framework.EfCore.Data;

namespace MasLazu.AspNet.Framework.EfCore.Postgresql.Data;

/// <summary>
/// PostgreSQL-specific design-time factory for creating DbContext instances during EF Core design-time operations.
/// </summary>
/// <typeparam name="TDbContext">The type of DbContext to create</typeparam>
public abstract class PsqlDesignTimeDbContextFactory<TDbContext> : DesignTimeDbContextFactory<TDbContext>
    where TDbContext : BaseDbContext
{
    /// <summary>
    /// Configures the DbContext options with PostgreSQL provider.
    /// </summary>
    /// <param name="optionsBuilder">The options builder</param>
    /// <param name="connectionString">The connection string</param>
    protected override void ConfigureOptions(DbContextOptionsBuilder<TDbContext> optionsBuilder, string connectionString)
    {
        optionsBuilder.UseNpgsql(connectionString, options =>
        {
            options.EnableRetryOnFailure();
            options.MigrationsAssembly(GetMigrationsAssemblyName());
        });
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    public abstract string GetMigrationsAssemblyName();
}
