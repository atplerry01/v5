namespace Whycespace.Platform.Api.Intelligence.Knowledge.Article;

public sealed record ArticleRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ArticleResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
