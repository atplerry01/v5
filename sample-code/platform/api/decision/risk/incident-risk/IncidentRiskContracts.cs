namespace Whycespace.Platform.Api.Decision.Risk.IncidentRisk;

public sealed record IncidentRiskRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record IncidentRiskResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
