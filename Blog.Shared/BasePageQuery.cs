namespace Blog.Shared;

public class BasePageQuery
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string? Search { get; set; }
    public BasePageQuery()
    {
        Page = 1;
    }
}