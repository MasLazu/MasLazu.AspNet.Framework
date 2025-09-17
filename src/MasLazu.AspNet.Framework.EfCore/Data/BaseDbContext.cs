using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MasLazu.AspNet.Framework.Domain.Entities;
using MasLazu.AspNet.Framework.EfCore.Configurations;

namespace MasLazu.AspNet.Framework.EfCore.Data;

/// <summary>
/// Base database context with automatic auditing, soft delete support, and transaction management
/// </summary>
public abstract class BaseDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the BaseDbContext class
    /// </summary>
    /// <param name="options">The options for this context</param>
    protected BaseDbContext(DbContextOptions options) : base(options)
    {
    }

    /// <summary>
    /// Configures the context options, including snake_case naming convention
    /// </summary>
    /// <param name="optionsBuilder">The options builder</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSnakeCaseNamingConvention();
    }
}
