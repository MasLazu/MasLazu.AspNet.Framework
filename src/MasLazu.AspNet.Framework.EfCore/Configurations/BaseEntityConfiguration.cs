using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasLazu.AspNet.Framework.Domain.Entities;

namespace MasLazu.AspNet.Framework.EfCore.Configurations;

/// <summary>
/// Base entity configuration for Entity Framework providing common configuration for all entities
/// </summary>
/// <typeparam name="T">The entity type that inherits from BaseEntity</typeparam>
public abstract class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity
{
    /// <summary>
    /// Configures the entity of type T
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type</param>
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.DeletedAt);

        builder.HasIndex(x => new { x.Id, x.DeletedAt });
    }
}
