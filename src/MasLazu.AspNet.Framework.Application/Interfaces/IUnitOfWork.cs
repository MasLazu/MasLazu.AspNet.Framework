namespace MasLazu.AspNet.Framework.Application.Interfaces;

/// <summary>
/// Interface for Unit of Work pattern providing transaction management
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all changes made in this unit of work to the database
    /// </summary>
    /// <param name="ct">A token to cancel the operation</param>
    /// <returns>The number of state entries written to the database</returns>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}