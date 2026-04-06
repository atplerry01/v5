namespace Whycespace.Platform.Api.Business.Integration.Synchronization;

public sealed record SynchronizationRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record SynchronizationResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
