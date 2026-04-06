namespace Whycespace.Platform.Api.Business.Logistic.Handoff;

public sealed record HandoffRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record HandoffResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
