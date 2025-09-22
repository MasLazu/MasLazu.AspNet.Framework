using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MasLazu.AspNet.Framework.Application.Interfaces;
using MasLazu.AspNet.Framework.Application.Models;
using MasLazu.AspNet.Framework.Application.Utils;
using MasLazu.AspNet.Framework.Domain.Entities;
using MasLazu.AspNet.Framework.EfCore.Data;

namespace MasLazu.AspNet.Framework.EfCore.Repositories;

public class ReadRepository<T, TContext> : IReadRepository<T>
    where T : BaseEntity
    where TContext : BaseReadDbContext
{
    protected readonly TContext _context;
    protected readonly DbSet<T> _dbSet;
    protected readonly ExpressionBuilder<T> _expressionBuilder;

    public ReadRepository(TContext context, ExpressionBuilder<T> expressionBuilder)
    {
        _expressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(e => e.DeletedAt == null)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public virtual async Task<List<T>> GetByIdsAsync(List<Guid> ids, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(e => e.DeletedAt == null && ids.Contains(e.Id))
            .ToListAsync(ct);
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet.Where(e => e.DeletedAt == null).AsQueryable();

        foreach (Expression<Func<T, object>> include in includes)
        {
            query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(e => e.Id == id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbSet
            .Where(e => e.DeletedAt == null)
            .ToListAsync(ct);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet.Where(e => e.DeletedAt == null).AsQueryable();

        foreach (Expression<Func<T, object>> include in includes)
        {
            query = query.Include(include);
        }

        return await query.ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(e => e.DeletedAt == null)
            .Where(predicate)
            .ToListAsync(ct);
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet.Where(e => e.DeletedAt == null).Where(predicate);

        foreach (Expression<Func<T, object>> include in includes)
        {
            query = query.Include(include);
        }

        return await query.ToListAsync();
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(e => e.DeletedAt == null)
            .FirstOrDefaultAsync(predicate, ct);
    }

    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(e => e.DeletedAt == null)
            .AnyAsync(e => e.Id == id, ct);
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(e => e.DeletedAt == null)
            .AnyAsync(predicate, ct);
    }

    public virtual async Task<int> CountAsync(CancellationToken ct = default)
    {
        return await _dbSet
            .Where(e => e.DeletedAt == null)
            .CountAsync(ct);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(e => e.DeletedAt == null)
            .CountAsync(predicate, ct);
    }

    public async virtual Task<(List<T>, int)> GetPaginatedAsync(PaginationRequest request, CancellationToken ct = default)
    {
        IQueryable<T> query = _dbSet.AsQueryable();

        query = _expressionBuilder.ApplyFilters(query, request.Filters);
        int totalCount = await query.CountAsync(ct);

        query = _expressionBuilder.ApplyOrdering(query, request.OrderBy);
        query = _expressionBuilder.ApplyPagination(query, request);
        List<T> items = await query.ToListAsync(ct);

        return (items, totalCount);
    }

    public async virtual Task<(List<T>, Guid?)> GetCursorPaginatedAsync(CursorPaginationRequest request, CancellationToken ct = default)
    {
        IQueryable<T> query = _dbSet.AsQueryable();

        query = _expressionBuilder.ApplyFilters(query, request.Filters);

        var orderings = request.OrderBy?.ToList();
        orderings ??= new() { new OrderBy { Field = "Id", Desc = false } };

        if (!orderings.Any(o => string.Equals(o.Field, "Id", StringComparison.OrdinalIgnoreCase)))
        {
            orderings.Add(new OrderBy { Field = "Id", Desc = false });
        }

        query = _expressionBuilder.ApplyOrdering(query, orderings);
        query = _expressionBuilder.ApplyCursor(query, request);

        List<T> items = await query.Take(request.Limit + 1).ToListAsync(ct);

        Guid? nextCursor = items.Count > request.Limit
            ? items.Last().Id
            : (Guid?)null;

        return (items.Take(request.Limit).ToList(), nextCursor);
    }

    public virtual async Task<IEnumerable<TimeseriesDataPoint>> GetTimeseriesCountAsync(TimeRange timeRange, TimeSpan interval, Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
    {
        IQueryable<T> query = _dbSet
            .Where(e => e.DeletedAt == null && e.CreatedAt >= timeRange.Start && e.CreatedAt <= timeRange.End);

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        List<T> entities = await query.ToListAsync(ct);

        IOrderedEnumerable<TimeseriesDataPoint> result = entities
            .GroupBy(e =>
            {
                long bucketTicks = e.CreatedAt.Ticks / interval.Ticks * interval.Ticks;
                return new DateTimeOffset(bucketTicks, e.CreatedAt.Offset);
            })
            .Select(g => new TimeseriesDataPoint { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date);

        return result;
    }
}
