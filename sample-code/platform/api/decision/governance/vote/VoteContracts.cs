namespace Whycespace.Platform.Api.Decision.Governance.Vote;

public sealed record VoteRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record VoteResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
