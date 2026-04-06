namespace Whycespace.Platform.Api.Business.Execution.Sourcing;

public sealed record SourcingRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record SourcingResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
