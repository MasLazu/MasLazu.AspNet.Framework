using FluentValidation;
using FluentValidation.Results;
using Mapster;
using MasLazu.AspNet.Framework.Application.Interfaces;
using MasLazu.AspNet.Framework.Application.Models;
using MasLazu.AspNet.Framework.Application.Utils;
using MasLazu.AspNet.Framework.Application.Exceptions;
using MasLazu.AspNet.Framework.Domain.Entities;
using FastEndpoints;

namespace MasLazu.AspNet.Framework.Application.Services;

/// <summary>
/// CRUD service implementation providing common business operations
/// </summary>
/// <typeparam name="TEntity">The entity type that inherits from BaseEntity</typeparam>
/// <typeparam name="TDto">The DTO type that inherits from BaseDto</typeparam>
/// <typeparam name="TCreateRequest">The create request type</typeparam>
/// <typeparam name="TUpdateRequest">The update request type that inherits from BaseUpdateRequest</typeparam>
public abstract class CrudService<TEntity, TDto, TCreateRequest, TUpdateRequest> : ICrudService<TDto, TCreateRequest, TUpdateRequest>
    where TEntity : BaseEntity
    where TDto : BaseDto
    where TUpdateRequest : BaseUpdateRequest
{
    protected readonly IRepository<TEntity> Repository;
    protected readonly IReadRepository<TEntity> ReadRepository;
    protected readonly IUnitOfWork UnitOfWork;
    protected readonly IValidator<TCreateRequest>? CreateValidator;
    protected readonly IValidator<TUpdateRequest>? UpdateValidator;
    protected readonly IPaginationValidator<TEntity> PaginationValidator;
    protected readonly ICursorPaginationValidator<TEntity> CursorPaginationValidator;
    protected readonly IEntityPropertyMap<TEntity> PropertyMap;

    protected CrudService(
        IRepository<TEntity> repository,
        IReadRepository<TEntity> readRepository,
        IUnitOfWork unitOfWork,
        IEntityPropertyMap<TEntity> propertyMap,
        IPaginationValidator<TEntity> paginationValidator,
        ICursorPaginationValidator<TEntity> cursorPaginationValidator,
        IValidator<TCreateRequest>? createValidator = null,
        IValidator<TUpdateRequest>? updateValidator = null)
    {
        Repository = repository;
        ReadRepository = readRepository;
        UnitOfWork = unitOfWork;
        PropertyMap = propertyMap;
        CreateValidator = createValidator;
        UpdateValidator = updateValidator;
        PaginationValidator = paginationValidator;
        CursorPaginationValidator = cursorPaginationValidator;
    }

    protected virtual async Task ValidateAsync<T>(T request, IValidator<T>? validator, CancellationToken ct = default)
    {
        if (validator != null)
        {
            ValidationResult validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                throw new ValidationFailureException(validationResult.Errors, "Validation failed");
            }
        }
    }

    public virtual async Task<TDto?> GetByIdAsync(Guid userId, Guid id, CancellationToken ct = default)
    {
        TEntity? entity = await ReadRepository.GetByIdAsync(id, ct);
        return entity.Adapt<TDto>();
    }

    public virtual async Task<IEnumerable<TDto>> GetAllAsync(Guid userId, CancellationToken ct = default)
    {
        IEnumerable<TEntity> entities = await ReadRepository.GetAllAsync(ct);
        return entities.Adapt<IEnumerable<TDto>>();
    }

    public virtual async Task<TDto> CreateAsync(Guid userId, TCreateRequest createRequest, bool saveChanges = true, CancellationToken ct = default)
    {
        await ValidateAsync(createRequest, CreateValidator, ct);

        TEntity entity = createRequest.Adapt<TEntity>();
        TEntity createdEntity = await Repository.AddAsync(entity, ct);

        if (saveChanges)
        {
            await UnitOfWork.SaveChangesAsync(ct);
        }

        return createdEntity.Adapt<TDto>();
    }

    public virtual async Task<TDto> CreateIfNotExistAsync(Guid id, TCreateRequest createRequest, bool saveChanges = true, CancellationToken ct = default)
    {
        await ValidateAsync(createRequest, CreateValidator, ct);

        TEntity? existingEntity = await Repository.GetByIdAsync(id, ct);
        if (existingEntity != null)
        {
            return existingEntity.Adapt<TDto>();
        }

        TEntity entity = createRequest.Adapt<TEntity>();
        TEntity createdEntity = await Repository.AddAsync(entity, ct);

        if (saveChanges)
        {
            await UnitOfWork.SaveChangesAsync(ct);
        }

        return createdEntity.Adapt<TDto>();
    }

    public virtual async Task<IEnumerable<TDto>> CreateRangeAsync(Guid userId, IEnumerable<TCreateRequest> createRequests, bool saveChanges = true, CancellationToken ct = default)
    {
        var requestList = createRequests.ToList();

        foreach (TCreateRequest? request in requestList)
        {
            await ValidateAsync(request, CreateValidator, ct);
        }

        List<TEntity> entities = requestList.Adapt<List<TEntity>>();
        IEnumerable<TEntity> createdEntities = await Repository.AddRangeAsync(entities, ct);

        if (saveChanges)
        {
            await UnitOfWork.SaveChangesAsync(ct);
        }

        return createdEntities.Adapt<IEnumerable<TDto>>();
    }

    public virtual async Task<TDto> UpdateAsync(Guid userId, TUpdateRequest updateRequest, bool saveChanges = true, CancellationToken ct = default)
    {
        TEntity? existingEntity = await Repository.GetByIdAsync(updateRequest.Id, ct) ??
            throw new NotFoundException(typeof(TEntity).Name, updateRequest.Id);

        await ValidateAsync(updateRequest, UpdateValidator, ct);

        TEntity updatedEntity = updateRequest.Adapt(existingEntity);
        updatedEntity.UpdatedAt = DateTimeOffset.UtcNow;

        await Repository.UpdateAsync(updatedEntity, ct);

        if (saveChanges)
        {
            await UnitOfWork.SaveChangesAsync(ct);
        }

        return updatedEntity.Adapt<TDto>();
    }

    public virtual async Task<IEnumerable<TDto>> UpdateRangeAsync(Guid userId, IEnumerable<TUpdateRequest> updateRequests, bool saveChanges = true, CancellationToken ct = default)
    {
        var requestList = updateRequests.ToList();
        var ids = requestList.Select(r => r.Id).ToList();

        List<TEntity> existingEntities = await Repository.GetByIdsAsync(ids, ct);
        var entityDict = existingEntities.ToDictionary(e => e.Id);

        var updatedEntities = new List<TEntity>();
        foreach (TUpdateRequest request in requestList)
        {
            if (!entityDict.TryGetValue(request.Id, out TEntity? existingEntity))
            {
                throw new NotFoundException(typeof(TEntity).Name, request.Id);
            }

            await ValidateAsync(request, UpdateValidator, ct);

            TEntity updatedEntity = request.Adapt(existingEntity);
            updatedEntity.UpdatedAt = DateTimeOffset.UtcNow;
            updatedEntities.Add(updatedEntity);
        }

        await Repository.UpdateRangeAsync(updatedEntities, ct);

        if (saveChanges)
        {
            await UnitOfWork.SaveChangesAsync(ct);
        }

        return updatedEntities.Adapt<IEnumerable<TDto>>();
    }

    public virtual async Task DeleteAsync(Guid userId, Guid id, bool saveChanges = true, CancellationToken ct = default)
    {
        TEntity? entity = await Repository.GetByIdAsync(id, ct) ??
            throw new NotFoundException(typeof(TEntity).Name, id);

        await Repository.DeleteAsync(entity, ct);
        await UnitOfWork.SaveChangesAsync(ct);
    }

    public virtual async Task DeleteRangeAsync(Guid userId, IEnumerable<Guid> ids, bool saveChanges = true, CancellationToken ct = default)
    {
        var idList = ids.ToList();

        List<TEntity> entities = await Repository.GetByIdsAsync(idList, ct);

        if (entities.Count != idList.Count)
        {
            var foundIds = entities.Select(e => e.Id).ToHashSet();
            IEnumerable<Guid> missingIds = idList.Where(id => !foundIds.Contains(id));
            throw new BadRequestException($"Entities with IDs [{string.Join(", ", missingIds)}] not found");
        }

        await Repository.DeleteRangeAsync(entities, ct);

        if (saveChanges)
        {
            await UnitOfWork.SaveChangesAsync(ct);
        }
    }

    public virtual async Task<bool> ExistsAsync(Guid userId, Guid id, CancellationToken ct = default)
    {
        return await ReadRepository.ExistsAsync(id, ct);
    }

    public virtual async Task<int> CountAsync(Guid userId, CancellationToken ct = default)
    {
        return await ReadRepository.CountAsync(ct);
    }

    public async Task<PaginatedResult<TDto>> GetPaginatedAsync(Guid userId, PaginationRequest request, CancellationToken ct = default)
    {
        await ValidateAsync(request.Adapt<PaginationRequest<TEntity>>(), PaginationValidator, ct);
        (List<TEntity> entities, int count) = await ReadRepository.GetPaginatedAsync(request, ct);
        return new PaginatedResult<TDto>
        {
            TotalCount = count,
            PageSize = request.PageSize,
            Page = request.Page,
            Items = entities.Adapt<List<TDto>>()
        };
    }

    public async Task<CursorPaginatedResult<TDto>> GetCursorPaginatedAsync(Guid userId, CursorPaginationRequest request, CancellationToken ct = default)
    {
        await ValidateAsync(request, CursorPaginationValidator, ct);
        (List<TEntity> entities, Guid? nextCursor) = await ReadRepository.GetCursorPaginatedAsync(request, ct);
        return new CursorPaginatedResult<TDto>
        {
            Items = entities.Adapt<List<TDto>>(),
            NextCursor = nextCursor?.ToString()
        };
    }
}