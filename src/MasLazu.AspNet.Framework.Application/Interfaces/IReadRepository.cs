using System.Linq.Expressions;
using MasLazu.AspNet.Framework.Application.Models;
using MasLazu.AspNet.Framework.Domain.Entities;

/// <summary>
/// ReadRepository interface providing common read access operations
/// </summary>
/// <typeparam name="T">The entity type that inherits from BaseEntity</typeparam>
public interface IReadRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Gets an entity by its identifier
    /// </summary>
    /// <param name="id">The entity identifier</param>
    /// <param name="ct">A token to cancel the operation</param>
    /// <returns>The entity if found, otherwise null</returns>
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets an entity by its identifier
    /// </summary>
    /// <param name="id">The entity identifier</param>
    /// <param name="ct">A token to cancel the operation</param>
    /// <returns>The entity if found, otherwise null</returns>
    Task<List<T>> GetByIdsAsync(List<Guid> ids, CancellationToken ct = default);

    /// <summary>
    /// Gets an entity by its identifier with related entities included
    /// </summary>
    /// <param name="id">The entity identifier</param>
    /// <param name="includes">The related entities to include</param>
    /// <returns>The entity if found, otherwise null</returns>
    Task<T?> GetByIdAsync(Guid id, params Expression<Func<T, object>>[] includes);

    /// <summary>
    /// Gets all entities
    /// </summary>
    /// <param name="ct">A token to cancel the operation</param>
    /// <returns>A collection of all entities</returns>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets all entities with related entities included
    /// </summary>
    /// <param name="includes">The related entities to include</param>
    /// <returns>A collection of all entities</returns>
    Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);

    // Querying operations

    /// <summary>
    /// Finds entities that match the specified predicate
    /// </summary>
    /// <param name="predicate">The predicate to match</param>
    /// <param name="ct">A token to cancel the operation</param>
    /// <returns>A collection of matching entities</returns>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    /// <summary>
    /// Finds entities that match the specified predicate with related entities included
    /// </summary>
    /// <param name="predicate">The predicate to match</param>
    /// <param name="includes">The related entities to include</param>
    /// <returns>A collection of matching entities</returns>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);

    /// <summary>
    /// Gets the first entity that matches the specified predicate, or null if no match is found
    /// </summary>
    /// <param name="predicate">The predicate to match</param>
    /// <param name="ct">A token to cancel the operation</param>
    /// <returns>The first matching entity, or null if no match is found</returns>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);


    /// <summary>
    /// Retrieves a paginated result set based on the specified pagination request.
    /// </summary>
    /// <param name="request">The pagination request containing page size, number, and filter criteria.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="PaginatedResult{T}"/> with the paginated data.</returns>
    Task<(List<T>, int)> GetPaginatedAsync(PaginationRequest request, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a cursor-based paginated result set based on the specified cursor pagination request.
    /// Cursor pagination is more efficient for large datasets and provides consistent pagination even when new items are added.
    /// </summary>
    /// <param name="request">The cursor pagination request containing limit, cursor field, cursor value, and filter criteria.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of entities ordered according to the cursor field.</returns>
    Task<(List<T>, Guid?)> GetCursorPaginatedAsync(CursorPaginationRequest request, CancellationToken ct = default);

    /// <summary>
    /// Checks if any entity matches the specified predicate
    /// </summary>
    /// <param name="predicate">The predicate to match</param>
    /// <param name="ct">A token to cancel the operation</param>
    /// <returns>True if any entity matches, otherwise false</returns>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    /// <summary>
    /// Gets the total count of entities
    /// </summary>
    /// <param name="ct">A token to cancel the operation</param>
    /// <returns>The total count of entities</returns>
    Task<int> CountAsync(CancellationToken ct = default);

    /// <summary>
    /// Checks if an entity with the specified identifier exists
    /// </summary>
    /// <param name="id">The entity identifier</param>
    /// <param name="ct">A token to cancel the operation</param>
    /// <returns>True if the entity exists, otherwise false</returns>
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets the count of entities that match the specified predicate
    /// </summary>
    /// <param name="predicate">The predicate to match</param>
    /// <param name="ct">A token to cancel the operation</param>
    /// <returns>The count of matching entities</returns>
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    /// <summary>
    /// Gets the timeseries count of entities within the specified time range, grouped by the sample period
    /// </summary>
    /// <param name="timeRange">The time range to query</param>
    /// <param name="interval">The sampling period (e.g., TimeSpan.FromHours(1) for hourly)</param>
    /// <param name="predicate">Optional predicate to filter entities</param>
    /// <param name="ct">A token to cancel the operation</param>
    /// <returns>A collection of timeseries data points containing the date and count for each period</returns>
    Task<IEnumerable<TimeseriesDataPoint>> GetTimeseriesCountAsync(TimeRange timeRange, TimeSpan interval, Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);
}