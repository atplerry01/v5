namespace Whycespace.Platform.Api.Business.Marketplace.Match;

public sealed record MatchRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record MatchResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
