namespace Whycespace.Shared.Contracts.Economic.Enforcement.Sanction;

public sealed record SanctionReadModel
{
    public Guid SanctionId { get; init; }
    public Guid SubjectId { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Scope { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTimeOffset EffectiveAt { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
    public DateTimeOffset IssuedAt { get; init; }
    public DateTimeOffset? ActivatedAt { get; init; }
    public DateTimeOffset? RevokedAt { get; init; }
    public DateTimeOffset? ExpiredAt { get; init; }
    public string RevocationReason { get; init; } = string.Empty;
    public DateTimeOffset LastUpdatedAt { get; init; }
}
