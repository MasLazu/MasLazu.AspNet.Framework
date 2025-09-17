using System.Linq.Expressions;
using MasLazu.AspNet.Framework.Domain.Entities;

namespace MasLazu.AspNet.Framework.Application.Interfaces;

public interface IEntityPropertyMap<TEntity> where TEntity : BaseEntity
{
    /// <summary>
    /// Get the expression for a given property string.
    /// </summary>
    /// <param name="property">Property name (case-insensitive)</param>
    /// <returns>Expression usable in EF Core LINQ</returns>
    Expression<Func<TEntity, object>> Get(string property);
}