using System.Transactions;
using Microsoft.EntityFrameworkCore;
using MasLazu.AspNet.Framework.Application.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace MasLazu.AspNet.Framework.EfCore.Data;

public class SharedTransactionUnitOfWork : IUnitOfWork
{
    private readonly IEnumerable<BaseDbContext> _dbContexts;

    public SharedTransactionUnitOfWork(IEnumerable<BaseDbContext> dbContexts)
    {
        _dbContexts = dbContexts;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var contextsList = _dbContexts.ToList();
        if (!contextsList.Any())
        {
            return 0;
        }

        BaseDbContext primaryContext = contextsList.First();

        using IDbContextTransaction transaction = await primaryContext.Database.BeginTransactionAsync(ct);

        try
        {
            int totalAffectedRows = 0;

            foreach (BaseDbContext? context in contextsList)
            {
                if (context != primaryContext)
                {
                    await context.Database.UseTransactionAsync(transaction.GetDbTransaction(), ct);
                }

                int affectedRows = await context.SaveChangesAsync(ct);
                totalAffectedRows += affectedRows;
            }

            await transaction.CommitAsync(ct);
            return totalAffectedRows;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<int> SaveChangesAsync(System.Data.IsolationLevel isolationLevel, CancellationToken ct = default)
    {
        var contextsList = _dbContexts.ToList();
        if (!contextsList.Any())
        {
            return 0;
        }

        BaseDbContext primaryContext = contextsList.First();

        using IDbContextTransaction transaction = await primaryContext.Database.BeginTransactionAsync(isolationLevel, ct);

        try
        {
            int totalAffectedRows = 0;

            foreach (BaseDbContext? context in contextsList)
            {
                if (context != primaryContext)
                {
                    await context.Database.UseTransactionAsync(transaction.GetDbTransaction(), ct);
                }

                totalAffectedRows += await context.SaveChangesAsync(ct);
            }

            await transaction.CommitAsync(ct);
            return totalAffectedRows;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<IEnumerable<BaseDbContext>, Task<T>> operation, CancellationToken ct = default)
    {
        var contextsList = _dbContexts.ToList();
        if (!contextsList.Any())
        {

            throw new InvalidOperationException("No DbContexts available");
        }


        BaseDbContext primaryContext = contextsList.First();

        using IDbContextTransaction transaction = await primaryContext.Database.BeginTransactionAsync(ct);

        try
        {
            foreach (BaseDbContext? context in contextsList.Skip(1))
            {
                await context.Database.UseTransactionAsync(transaction.GetDbTransaction(), ct);
            }

            T result = await operation(contextsList);
            await transaction.CommitAsync(ct);
            return result;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task ExecuteInTransactionAsync(Func<IEnumerable<BaseDbContext>, Task> operation, CancellationToken ct = default)
    {
        var contextsList = _dbContexts.ToList();
        if (!contextsList.Any())
        {
            return;
        }


        BaseDbContext primaryContext = contextsList.First();

        using IDbContextTransaction transaction = await primaryContext.Database.BeginTransactionAsync(ct);

        try
        {
            foreach (BaseDbContext? context in contextsList.Skip(1))
            {
                await context.Database.UseTransactionAsync(transaction.GetDbTransaction(), ct);
            }

            await operation(contextsList);
            await transaction.CommitAsync(ct);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}