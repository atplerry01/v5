namespace Whycespace.Shared.Contracts.Common;

/// <summary>
/// Standard pagination response. List endpoints return ApiResponse&lt;PagedResponse&lt;T&gt;&gt;.
/// </summary>
public sealed class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public PageMeta Page { get; set; } = new();
}

/// <summary>
/// Pagination metadata describing the current page window and total extent.
/// </summary>
public sealed class PageMeta
{
    public int Number { get; set; }
    public int Size { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}

/// <summary>
/// Standard pagination request parameters for list endpoints.
/// </summary>
public sealed record PagedRequest(int Page = 1, int PageSize = 20);
