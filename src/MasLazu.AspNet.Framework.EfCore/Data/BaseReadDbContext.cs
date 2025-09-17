using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MasLazu.AspNet.Framework.Domain.Entities;
using MasLazu.AspNet.Framework.EfCore.Configurations;

namespace MasLazu.AspNet.Framework.EfCore.Data;

/// <summary>
/// Base read-only database context optimized for query operations
/// </summary>
public abstract class BaseReadDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the BaseReadDbContext class
    /// </summary>
    /// <param name="options">The options for this context</param>
    protected BaseReadDbContext(DbContextOptions options) : base(options)
    {
        ChangeTracker.AutoDetectChangesEnabled = false;
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    /// <summary>
    /// Configures the context options, including snake_case naming convention and read optimizations
    /// </summary>
    /// <param name="optionsBuilder">The options builder</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSnakeCaseNamingConvention();
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }

    /// <summary>
    /// Saves changes to the database (not supported in read-only context)
    /// </summary>
    /// <exception cref="InvalidOperationException">Always thrown as this is a read-only context</exception>
    public override int SaveChanges()
    {
        throw new InvalidOperationException("This is a read-only database context. Use a writable context for save operations.");
    }

    /// <summary>
    /// Saves changes to the database asynchronously (not supported in read-only context)
    /// </summary>
    /// <exception cref="InvalidOperationException">Always thrown as this is a read-only context</exception>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("This is a read-only database context. Use a writable context for save operations.");
    }

    /// <summary>
    /// Saves changes to the database asynchronously (not supported in read-only context)
    /// </summary>
    /// <exception cref="InvalidOperationException">Always thrown as this is a read-only context</exception>
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("This is a read-only database context. Use a writable context for save operations.");
    }

    /// <summary>
    /// Executes a query with no-tracking behavior for better read performance
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <returns>An IQueryable with no-tracking behavior</returns>
    public IQueryable<TEntity> Query<TEntity>() where TEntity : class
    {
        return Set<TEntity>().AsNoTracking();
    }

    /// <summary>
    /// Executes a query with no-tracking behavior and includes related entities
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <param name="includeProperties">Navigation properties to include</param>
    /// <returns>An IQueryable with no-tracking behavior and includes</returns>
    public IQueryable<TEntity> Query<TEntity>(params Expression<Func<TEntity, object>>[] includeProperties) where TEntity : class
    {
        IQueryable<TEntity> query = Set<TEntity>().AsNoTracking();

        foreach (Expression<Func<TEntity, object>> includeProperty in includeProperties)
        {
            query = query.Include(includeProperty);
        }

        return query;
    }
}
