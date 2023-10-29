namespace CoreLib.EntityFramework.Features.SearchPagination;

public interface IPagedQuery
{
    public int? Limit { get; set; }
    public int? Offset { get; set; }
}
