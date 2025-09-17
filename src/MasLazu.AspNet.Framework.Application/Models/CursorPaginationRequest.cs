namespace MasLazu.AspNet.Framework.Application.Models;

public class CursorPaginationRequest
{
    public int Limit { get; set; } = 10;
    public Guid? Cursor { get; set; }
    public List<Filter> Filters { get; set; } = new();
    public List<OrderBy> OrderBy { get; set; } = new();
}
