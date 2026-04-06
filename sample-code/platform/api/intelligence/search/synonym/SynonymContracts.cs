namespace Whycespace.Platform.Api.Intelligence.Search.Synonym;

public sealed record SynonymRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record SynonymResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
