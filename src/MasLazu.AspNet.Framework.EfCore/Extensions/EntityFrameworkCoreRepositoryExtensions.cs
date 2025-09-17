using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MasLazu.AspNet.Framework.Application.Interfaces;
using MasLazu.AspNet.Framework.Application.Utils;
using MasLazu.AspNet.Framework.Domain.Entities;
using MasLazu.AspNet.Framework.EfCore.Data;
using MasLazu.AspNet.Framework.EfCore.Repositories;

namespace MasLazu.AspNet.Framework.EfCore.Extensions;

/// <summary>
/// Extension methods for automatically registering repositories for all DbContext and entity types.
/// </summary>
public static class EntityFrameworkCoreRepositoryExtensions
{
    /// <summary>
    /// Automatically registers repositories for all BaseEntity types found across all DbContexts.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">The assemblies to scan for types. If not provided, scans all loaded assemblies.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddAutoRepositories(
        this IServiceCollection services,
        Assembly[]? assemblies = null)
    {
        assemblies ??= AppDomain.CurrentDomain.GetAssemblies();

        RegisterAllRepositories(services, assemblies);

        return services;
    }

    /// <summary>
    /// Automatically registers repositories for specific assemblies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblyNames">The names of assemblies to scan.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddAutoRepositoriesFromAssemblies(
        this IServiceCollection services,
        string[] assemblyNames)
    {
        Assembly[] assemblies = assemblyNames
            .Select(name => Assembly.Load(name))
            .ToArray();

        return services.AddAutoRepositories(assemblies);
    }

    /// <summary>
    /// Registers repositories for all BaseEntity types found across all DbContexts.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">The assemblies to scan.</param>
    private static void RegisterAllRepositories(IServiceCollection services, Assembly[] assemblies)
    {
        List<Type> dbContextTypes = new();
        List<Type> readDbContextTypes = new();

        foreach (Assembly assembly in assemblies)
        {
            Type[] writeContexts = assembly.GetTypes()
                .Where(type => !type.IsAbstract &&
                              !type.IsInterface &&
                              typeof(BaseDbContext).IsAssignableFrom(type) &&
                              !typeof(BaseReadDbContext).IsAssignableFrom(type))
                .ToArray();
            dbContextTypes.AddRange(writeContexts);

            Type[] readContexts = assembly.GetTypes()
                .Where(type => !type.IsAbstract &&
                              !type.IsInterface &&
                              typeof(BaseReadDbContext).IsAssignableFrom(type))
                .ToArray();
            readDbContextTypes.AddRange(readContexts);
        }

        HashSet<Type> entityTypes = new();
        foreach (Type contextType in dbContextTypes.Concat(readDbContextTypes))
        {
            PropertyInfo[] dbSetProperties = contextType.GetProperties()
                .Where(prop => prop.PropertyType.IsGenericType &&
                              prop.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .ToArray();

            foreach (PropertyInfo prop in dbSetProperties)
            {
                Type entityType = prop.PropertyType.GetGenericArguments()[0];
                if (typeof(BaseEntity).IsAssignableFrom(entityType))
                {
                    entityTypes.Add(entityType);
                }
            }
        }

        foreach (Type entityType in entityTypes)
        {
            foreach (Type dbContextType in dbContextTypes)
            {
                if (HasEntityInContext(dbContextType, entityType))
                {
                    RegisterWriteRepository(services, entityType, dbContextType);
                }
            }

            foreach (Type readDbContextType in readDbContextTypes)
            {
                if (HasEntityInContext(readDbContextType, entityType))
                {
                    RegisterReadRepository(services, entityType, readDbContextType);
                }
            }
        }

        foreach (Type entityType in entityTypes)
        {
            Type expressionBuilderType = typeof(ExpressionBuilder<>).MakeGenericType(entityType);
            services.AddScoped(expressionBuilderType);
        }
    }

    private static bool HasEntityInContext(Type contextType, Type entityType)
    {
        return contextType.GetProperties()
            .Any(prop => prop.PropertyType.IsGenericType &&
                        prop.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>) &&
                        prop.PropertyType.GetGenericArguments()[0] == entityType);
    }

    private static void RegisterWriteRepository(IServiceCollection services, Type entityType, Type dbContextType)
    {
        Type repositoryInterfaceType = typeof(IRepository<>).MakeGenericType(entityType);
        Type readRepositoryInterfaceType = typeof(IReadRepository<>).MakeGenericType(entityType);
        Type repositoryImplementationType = typeof(Repository<,>).MakeGenericType(entityType, dbContextType);

        services.AddScoped(repositoryInterfaceType, repositoryImplementationType);
        services.AddScoped(readRepositoryInterfaceType, sp => sp.GetRequiredService(repositoryInterfaceType));
    }

    private static void RegisterReadRepository(IServiceCollection services, Type entityType, Type readDbContextType)
    {
        Type readRepositoryInterfaceType = typeof(IReadRepository<>).MakeGenericType(entityType);
        Type readRepositoryImplementationType = typeof(ReadRepository<,>).MakeGenericType(entityType, readDbContextType);

        services.AddScoped(readRepositoryInterfaceType, readRepositoryImplementationType);
    }
}
