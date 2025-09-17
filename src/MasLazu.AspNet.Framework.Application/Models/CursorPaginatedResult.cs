namespace MasLazu.AspNet.Framework.Application.Models;

public class CursorPaginatedResult<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public string? NextCursor { get; set; }
    public string? PreviousCursor { get; set; }
}
