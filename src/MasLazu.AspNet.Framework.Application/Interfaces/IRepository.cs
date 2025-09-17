using System.Linq.Expressions;
using MasLazu.AspNet.Framework.Application.Models;
using MasLazu.AspNet.Framework.Domain.Entities;

namespace MasLazu.AspNet.Framework.Application.Interfaces;

/// <summary>
/// Repository interface providing common read and write access operations
/// </summary>
/// <typeparam name="T">The entity type that inherits from BaseEntity</typeparam>
public interface IRepository<T> : IReadRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Adds a new entity
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <param name="ct">A token to cancel the operation</param>
    /// <returns>The added entity</returns>
    Task<T> AddAsync(T entity, CancellationToken ct = default);

    /// <summary>
    /// Adds multiple entities
    /// </summary>
    /// <param name="entities">The entities to add</param>
    /// <param name="ct">A token to cancel the operation</param>
    /// <returns>The added entities</returns>
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <param name="ct">A token to cancel the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task UpdateAsync(T entity, CancellationToken ct = default);

    /// <summary>
    /// Updates multiple entities
    /// </summary>
    /// <param name="entities">The entities to update</param>
    /// <param name="ct">A token to cancel the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

    /// <summary>
    /// Deletes an entity by its identifier
    /// </summary>
    /// <param name="id">The entity identifier</param>
    /// <param name="ct">A token to cancel the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Deletes an entity
    /// </summary>
    /// <param name="entity">The entity to delete</param>
    /// <param name="ct">A token to cancel the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task DeleteAsync(T entity, CancellationToken ct = default);

    /// <summary>
    /// Deletes multiple entities
    /// </summary>
    /// <param name="entities">The entities to delete</param>
    /// <param name="ct">A token to cancel the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
}