namespace Whycespace.Platform.Api.Business.Entitlement.Allocation;

public sealed record AllocationRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record AllocationResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
