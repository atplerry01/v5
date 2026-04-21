namespace Whycespace.Shared.Contracts.Content.Document.Governance.Retention;

public sealed record RetentionReadModel
{
    public Guid RetentionId { get; init; }
    public Guid TargetId { get; init; }
    public string TargetKind { get; init; } = string.Empty;
    public DateTimeOffset WindowAppliedAt { get; init; }
    public DateTimeOffset WindowExpiresAt { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset AppliedAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
