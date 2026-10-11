namespace Blog.Shared;

public class BasePageQueryResult<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<T> Data { get; set; }
    public long Total { get; set; }
}