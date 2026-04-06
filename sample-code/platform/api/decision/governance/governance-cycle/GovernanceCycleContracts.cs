namespace Whycespace.Platform.Api.Decision.Governance.GovernanceCycle;

public sealed record GovernanceCycleRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record GovernanceCycleResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
