namespace MasLazu.AspNet.Framework.Application.Models;

/// <summary>
/// Result model for paginated data
/// </summary>
/// <typeparam name="T">The type of items in the result</typeparam>
public class PaginatedResult<T> where T : BaseDto
{
    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Items for the current page
    /// </summary>
    public List<T> Items { get; set; } = new();
}
