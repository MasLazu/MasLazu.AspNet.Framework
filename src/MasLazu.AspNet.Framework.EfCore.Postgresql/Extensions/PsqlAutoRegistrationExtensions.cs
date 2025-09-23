using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MasLazu.AspNet.Framework.EfCore.Data;
using MasLazu.AspNet.Framework.EfCore.Extensions;

namespace MasLazu.AspNet.Framework.EfCore.Postgresql.Extensions;

/// <summary>
/// Extension methods for automatically registering all DbContext and ReadDbContext types with PostgreSQL provider.
/// </summary>
public static class PsqlAutoRegistrationExtensions
{
    /// <summary>
    /// Automatically registers all DbContext types that inherit from BaseDbContext with PostgreSQL provider.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration containing connection strings.</param>
    /// <param name="assemblies">The assemblies to scan for DbContext types. If not provided, scans all loaded assemblies.</param>
    /// <param name="lifetime">The lifetime of the DbContext services.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddAllPsqlReadWriteDbContexts(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly[]? assemblies = null,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        assemblies ??= AppDomain.CurrentDomain.GetAssemblies();

        foreach (Assembly assembly in assemblies)
        {
            Type[] dbContextTypes = assembly.GetTypes()
                .Where(type => !type.IsAbstract &&
                              !type.IsInterface &&
                              type.IsSubclassOf(typeof(DbContext)) &&
                              !typeof(BaseReadDbContext).IsAssignableFrom(type))
                .ToArray();

            foreach (Type dbContextType in dbContextTypes)
            {
                RegisterDbContext(services, configuration, dbContextType, lifetime);
            }
        }

        return services;
    }

    /// <summary>
    /// Automatically registers all ReadDbContext types that inherit from BaseReadDbContext with PostgreSQL provider.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration containing connection strings.</param>
    /// <param name="assemblies">The assemblies to scan for ReadDbContext types. If not provided, scans all loaded assemblies.</param>
    /// <param name="lifetime">The lifetime of the DbContext services.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddAllPsqlReadDbContexts(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly[]? assemblies = null,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        assemblies ??= AppDomain.CurrentDomain.GetAssemblies();

        foreach (Assembly assembly in assemblies)
        {
            Type[] readDbContextTypes = assembly.GetTypes()
                .Where(type => !type.IsAbstract &&
                              !type.IsInterface &&
                              typeof(BaseReadDbContext).IsAssignableFrom(type))
                .ToArray();

            foreach (Type readDbContextType in readDbContextTypes)
            {
                RegisterReadDbContext(services, configuration, readDbContextType, lifetime);
            }
        }

        return services;
    }

    /// <summary>
    /// Automatically registers all DbContext and ReadDbContext types with PostgreSQL provider, plus repositories.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration containing connection strings.</param>
    /// <param name="assemblies">The assemblies to scan for DbContext types. If not provided, scans all loaded assemblies.</param>
    /// <param name="lifetime">The lifetime of the DbContext services.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddAllPsqlDbContexts(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly[]? assemblies = null,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        services.AddAllPsqlReadWriteDbContexts(configuration, assemblies, lifetime);
        services.AddAllPsqlReadDbContexts(configuration, assemblies, lifetime);

        return services;
    }

    /// <summary>
    /// Registers DbContext and ReadDbContext types from specific assemblies with PostgreSQL provider.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration containing connection strings.</param>
    /// <param name="assemblyNames">The names of assemblies to scan.</param>
    /// <param name="lifetime">The lifetime of the DbContext services.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddPsqlContextsFromAssemblies(
        this IServiceCollection services,
        IConfiguration configuration,
        string[] assemblyNames,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        Assembly[] assemblies = assemblyNames
            .Select(name => Assembly.Load(name))
            .ToArray();

        return services.AddAllPsqlDbContexts(configuration, assemblies, lifetime);
    }

    private static void RegisterDbContext(
        IServiceCollection services,
        IConfiguration configuration,
        Type dbContextType,
        ServiceLifetime lifetime)
    {
        string? connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string 'DefaultConnection' not found for {dbContextType.Name}.");
        }

        MethodInfo addDbContextMethod = typeof(EntityFrameworkServiceCollectionExtensions)
            .GetMethods()
            .Where(m => m.Name == "AddDbContext" && m.GetParameters().Length == 4)
            .First()
            .MakeGenericMethod(dbContextType);

        addDbContextMethod.Invoke(null, new object[]
        {
            services,
            new Action<DbContextOptionsBuilder>(options =>
            {
                options.UseNpgsql(connectionString);
                options.UseSnakeCaseNamingConvention();
            }),
            lifetime,
            ServiceLifetime.Singleton
        });

        services.Add(new ServiceDescriptor(typeof(DbContext), sp => (DbContext)sp.GetRequiredService(dbContextType), lifetime));
        if (typeof(BaseDbContext).IsAssignableFrom(dbContextType))
        {
            services.Add(new ServiceDescriptor(typeof(BaseDbContext), sp => (BaseDbContext)sp.GetRequiredService(dbContextType), lifetime));
        }
    }

    private static void RegisterReadDbContext(
        IServiceCollection services,
        IConfiguration configuration,
        Type readDbContextType,
        ServiceLifetime lifetime)
    {
        string? connectionString = configuration.GetConnectionString("ReadConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string 'ReadConnection' not found for {readDbContextType.Name}.");
        }

        MethodInfo addDbContextMethod = typeof(EntityFrameworkServiceCollectionExtensions)
            .GetMethods()
            .Where(m => m.Name == "AddDbContext" && m.GetParameters().Length == 4)
            .First()
            .MakeGenericMethod(readDbContextType);

        addDbContextMethod.Invoke(null,
        [
            services,
            new Action<DbContextOptionsBuilder>(options =>
            {
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure();
                    npgsqlOptions.CommandTimeout(30);
                });
                options.UseSnakeCaseNamingConvention();
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }),
            lifetime,
            ServiceLifetime.Singleton
        ]);

        services.Add(new ServiceDescriptor(typeof(DbContext), sp => (DbContext)sp.GetRequiredService(readDbContextType), lifetime));
    }
}
