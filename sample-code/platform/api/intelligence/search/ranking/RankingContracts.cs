namespace Whycespace.Platform.Api.Intelligence.Search.Ranking;

public sealed record RankingRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record RankingResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
