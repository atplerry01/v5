namespace Whycespace.Projections.Global.GovernanceAudit;

public sealed record GovernanceAuditReadModel
{
    public required string Id { get; init; }
    public required string Action { get; init; }
    public required string ActorId { get; init; }
    public string? SuggestionId { get; init; }
    public string? PolicyId { get; init; }
    public string? Outcome { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public DateTimeOffset LastEventTimestamp { get; init; }
    public long LastEventVersion { get; init; }
}
