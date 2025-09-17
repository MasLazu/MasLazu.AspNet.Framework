using MasLazu.AspNet.Framework.Domain.Entities;

namespace MasLazu.AspNet.Framework.Application.Models;

public class PaginationRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public List<Filter> Filters { get; set; } = new();
    public List<OrderBy> OrderBy { get; set; } = new();
}

public class PaginationRequest<T> : PaginationRequest
    where T : BaseEntity
{
}

public class Filter
{
    /// <summary>
    /// Field name to filter on
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// Operator to use for filtering (=, !=, >, <, >=, <=, contains, startswith, endswith)
    /// </summary>
    public string Operator { get; set; } = "=";

    /// <summary>
    /// Value to filter by
    /// </summary>
    public string Value { get; set; } = string.Empty;
}
public class OrderBy
{
    /// <summary>
    /// Field name to sort by
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// Whether to sort in descending order
    /// </summary>
    public bool Desc { get; set; }
}
