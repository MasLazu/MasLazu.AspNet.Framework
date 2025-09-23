using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MasLazu.AspNet.Framework.EfCore.Data;
using MasLazu.AspNet.Framework.EfCore.Postgresql.Data;

namespace MasLazu.AspNet.Framework.EfCore.Postgresql.Utils;

/// <summary>
/// Utility class for scanning and instantiating PostgreSQL DbContext factories at runtime.
/// </summary>
public static class PsqlDbContextFactoryUtil
{
    /// <summary>
    /// Gets all read-write DbContext types from the specified assemblies.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan. If null, scans all loaded assemblies.</param>
    /// <returns>An enumerable of DbContext types that are read-write (not BaseReadDbContext).</returns>
    public static IEnumerable<Type> GetAllReadWriteDbContextTypes(Assembly[]? assemblies = null)
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
                yield return dbContextType;
            }
        }
    }

    /// <summary>
    /// Gets all read-only DbContext types from the specified assemblies.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan. If null, scans all loaded assemblies.</param>
    /// <returns>An enumerable of DbContext types that are read-only (BaseReadDbContext).</returns>
    public static IEnumerable<Type> GetAllReadDbContextTypes(Assembly[]? assemblies = null)
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
                yield return readDbContextType;
            }
        }
    }

    /// <summary>
    /// Creates a design-time DbContext factory for the specified DbContext type.
    /// </summary>
    /// <param name="dbContextType">The DbContext type.</param>
    /// <returns>An instance of the design-time factory (as object).</returns>
    public static object CreateDesignTimeFactory(Type dbContextType)
    {
        if (!dbContextType.IsSubclassOf(typeof(DbContext)))
        {
            throw new ArgumentException("Type must be a subclass of DbContext", nameof(dbContextType));
        }

        // Create the generic factory type, e.g., PsqlDesignTimeDbContextFactory<MyDbContext>
        Type factoryType = typeof(PsqlDesignTimeDbContextFactory<>).MakeGenericType(dbContextType);

        // Instantiate the factory
        object factoryInstance = Activator.CreateInstance(factoryType)!;

        return factoryInstance;
    }

    /// <summary>
    /// Creates design-time DbContext factories for all read-write DbContext types.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan. If null, scans all loaded assemblies.</param>
    /// <returns>An enumerable of factory instances (as objects).</returns>
    public static IEnumerable<object> CreateAllReadWriteDesignTimeFactories(Assembly[]? assemblies = null)
    {
        foreach (Type dbContextType in GetAllReadWriteDbContextTypes(assemblies))
        {
            yield return CreateDesignTimeFactory(dbContextType);
        }
    }

    /// <summary>
    /// Gets a design-time DbContext factory for the specified DbContext type using generics.
    /// </summary>
    /// <typeparam name="TDbContext">The DbContext type.</typeparam>
    /// <returns>An instance of IDesignTimeDbContextFactory for the DbContext type.</returns>
    public static IDesignTimeDbContextFactory<TDbContext> GetDbContextFactory<TDbContext>()
        where TDbContext : BaseDbContext
    {
        return (IDesignTimeDbContextFactory<TDbContext>)CreateDesignTimeFactory(typeof(TDbContext));
    }
}