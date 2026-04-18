namespace IncognitoPAS.DTOs.Api;

public class ApiErrorDto
{
    public string Message { get; set; } = string.Empty;
    public string? Code { get; set; }
}

public class PagedResult<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
}
