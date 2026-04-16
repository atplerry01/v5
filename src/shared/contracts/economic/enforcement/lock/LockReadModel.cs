namespace Whycespace.Shared.Contracts.Economic.Enforcement.Lock;

public sealed record LockReadModel
{
    public Guid LockId { get; init; }
    public Guid SubjectId { get; init; }
    public string Scope { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTimeOffset LockedAt { get; init; }
    public DateTimeOffset? UnlockedAt { get; init; }
    public string UnlockReason { get; init; } = string.Empty;
    public DateTimeOffset LastUpdatedAt { get; init; }
}
