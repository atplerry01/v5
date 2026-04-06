namespace Whycespace.Platform.Api.Orchestration.Workflow.Instance;

public sealed record InstanceRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record InstanceResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
