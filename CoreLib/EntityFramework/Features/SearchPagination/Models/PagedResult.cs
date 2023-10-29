namespace CoreLib.EntityFramework.Features.SearchPagination;

public class PagedResult<T>
{
    public long TotalItems { get; set; }
    public long ItemsQuantity { get; set; }
    public int ItemsOffset { get; set; }
    public IEnumerable<T> Items { get; set; } = Array.Empty<T>();
}
