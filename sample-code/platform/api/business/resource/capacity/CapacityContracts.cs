namespace Whycespace.Platform.Api.Business.Resource.Capacity;

public sealed record CapacityRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record CapacityResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
