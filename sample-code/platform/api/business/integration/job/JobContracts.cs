namespace Whycespace.Platform.Api.Business.Integration.Job;

public sealed record JobRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record JobResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
