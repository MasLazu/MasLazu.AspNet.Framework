using MasLazu.AspNet.Framework.Application.Models;
using MasLazu.AspNet.Framework.Domain.Entities;

namespace MasLazu.AspNet.Framework.Application.Interfaces;

/// <summary>
/// CRUD with authorization service interface providing common business operations
/// </summary>
/// <typeparam name="TEntity">The entity type that inherits from BaseEntity</typeparam>
/// <typeparam name="TDto">The DTO type that inherits from BaseDto</typeparam>
/// <typeparam name="TCreateRequest">The create request type</typeparam>
/// <typeparam name="TUpdateRequest">The update request type that inherits from BaseUpdateRequest</typeparam>
public interface ICrudWithAuthorizationService<TDto, TCreateRequest, TUpdateRequest>
    where TDto : BaseDto
    where TUpdateRequest : BaseUpdateRequest
{
    /// <summary>
    /// Gets a DTO by its identifier
    /// </summary>
    /// <param name="userId">The user identifier performing the operation</param>
    /// <param name="id">The entity identifier</param>
    /// <param name="ct">A token to cancel the operation</param>
    /// <returns>The DTO if found, otherwise null</returns>
    Task<TDto?> GetByIdAsync(Guid userId, Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets all DTOs
    /// </summary>
    /// <param name="userId">The user identifier performing the operation</param>
    /// <param name="ct">A token to cancel the operation</param>
    /// <returns>A collection of all DTOs</returns>
    Task<IEnumerable<TDto>> GetAllAsync(Guid userId, CancellationToken ct = default);

    // Write operations

    /// <summary>
    /// Creates a new entity from the create request
    /// </summary>
    /// <param name="userId">The user identifier performing the operation</param>
    /// <param name="createRequest">The create request</param>
    /// <param name="ct">A token to cancel the operation</param>
    /// <returns>The created DTO</returns>
    Task<TDto> CreateAsync(Guid userId, TCreateRequest createRequest, bool saveChanges = true, CancellationToken ct = default);

    /// <summary>
    /// Creates multiple entities from the create requests
    /// </summary>
    /// <param name="userId">The user identifier performing the operation</param>
    /// <param name="createRequests">The create requests</param>
    /// <param name="ct">A token to cancel the operation</param>
    /// <returns>The created DTOs</returns>
    Task<IEnumerable<TDto>> CreateRangeAsync(Guid userId, IEnumerable<TCreateRequest> createRequests, bool saveChanges = true, CancellationToken ct = default);

    /// <summary>
    /// Creates a new entity from the create request if an entity with the specified ID does not already exist
    /// </summary>
    /// <param name="id">The entity identifier to check for existence</param>
    /// <param name="createRequest">The create request</param>
    /// <param name="ct">A token to cancel the operation</param>
    /// <returns>The created DTO if the entity did not exist, otherwise throws an exception</returns>
    Task<TDto> CreateIfNotExistAsync(Guid id, TCreateRequest createRequest, bool saveChanges = true, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing entity with the update request
    /// </summary>
    /// <param name="userId">The user identifier performing the operation</param>
    /// <param name="updateRequest">The update request</param>
    /// <param name="ct">A token to cancel the operation</param>
    /// <returns>The updated DTO</returns>
    Task<TDto> UpdateAsync(Guid userId, TUpdateRequest updateRequest, bool saveChanges = true, CancellationToken ct = default);

    /// <summary>
    /// Updates multiple entities with the update requests
    /// </summary>
    /// <param name="userId">The user identifier performing the operation</param>
    /// <param name="updateRequests">The update requests</param>
    /// <param name="ct">A token to cancel the operation</param>
    /// <returns>The updated DTOs</returns>
    Task<IEnumerable<TDto>> UpdateRangeAsync(Guid userId, IEnumerable<TUpdateRequest> updateRequests, bool saveChanges = true, CancellationToken ct = default);

    /// <summary>
    /// Deletes an entity by its identifier
    /// </summary>
    /// <param name="userId">The user identifier performing the operation</param>
    /// <param name="id">The entity identifier</param>
    /// <param name="ct">A token to cancel the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task DeleteAsync(Guid userId, Guid id, bool saveChanges = true, CancellationToken ct = default);

    /// <summary>
    /// Deletes multiple entities by their identifiers
    /// </summary>
    /// <param name="userId">The user identifier performing the operation</param>
    /// <param name="ids">The entity identifiers</param>
    /// <param name="ct">A token to cancel the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task DeleteRangeAsync(Guid userId, IEnumerable<Guid> ids, bool saveChanges = true, CancellationToken ct = default);

    // Utility operations

    /// <summary>
    /// Checks if an entity with the specified identifier exists
    /// </summary>
    /// <param name="userId">The user identifier performing the operation</param>
    /// <param name="id">The entity identifier</param>
    /// <param name="ct">A token to cancel the operation</param>
    /// <returns>True if the entity exists, otherwise false</returns>
    Task<bool> ExistsAsync(Guid userId, Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets the total count of entities
    /// </summary>
    /// <param name="userId">The user identifier performing the operation</param>
    /// <param name="ct">A token to cancel the operation</param>
    /// <returns>The total count of entities</returns>
    Task<int> CountAsync(Guid userId, CancellationToken ct = default);

    /// <param name="userId">User performing the operation (used for authorization/audit).</param>
    /// <param name="request">Pagination request (page, page size, filters, orderings).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="PaginatedResult{TDto}"/> containing items and paging metadata.</returns>
    Task<PaginatedResult<TDto>> GetPaginatedAsync(Guid userId, PaginationRequest request, CancellationToken ct = default);

    /// <summary>
    /// Gets a cursor-paginated result set of DTOs
    /// </summary>
    /// <param name="userId">User performing the operation (used for authorization/audit).</param>
    /// <param name="request">Cursor pagination request (limit, cursor, filters, orderings).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="CursorPaginatedResult{TDto}"/> containing items and cursor information.</returns>
    Task<CursorPaginatedResult<TDto>> GetCursorPaginatedAsync(Guid userId, CursorPaginationRequest request, CancellationToken ct = default);
}