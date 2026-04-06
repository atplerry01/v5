namespace Whycespace.Platform.Api.Operational.Global.IncidentResponse;

public sealed record IncidentResponseRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record IncidentResponseResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
