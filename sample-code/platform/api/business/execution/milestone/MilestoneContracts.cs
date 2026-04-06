namespace Whycespace.Platform.Api.Business.Execution.Milestone;

public sealed record MilestoneRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record MilestoneResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
