using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

public sealed class EmergencyOverride
{
    public Guid OverrideId { get; }
    public string Reason { get; }
    public IReadOnlyList<Guid> ApprovedBy { get; }
    public DateTimeOffset ExpiresAt { get; }
    public string Scope { get; }
    public DateTimeOffset CreatedAt { get; }

    private EmergencyOverride(Guid overrideId, string reason, IReadOnlyList<Guid> approvedBy,
        DateTimeOffset expiresAt, string scope, DateTimeOffset createdAt)
    {
        OverrideId = overrideId;
        Reason = reason;
        ApprovedBy = approvedBy;
        ExpiresAt = expiresAt;
        Scope = scope;
        CreatedAt = createdAt;
    }

    public static EmergencyOverride Create(
        string reason, IReadOnlyList<Guid> approvedBy, DateTimeOffset expiresAt, string scope, DateTimeOffset timestamp)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        if (approvedBy.Count < 2) throw new InvalidOperationException("Emergency override requires at least 2 approvers.");

        return new EmergencyOverride(
            DeterministicIdHelper.FromSeed($"EmergencyOverride:{reason}:{scope}:{string.Join(",", approvedBy)}"), reason, approvedBy, expiresAt, scope, timestamp);
    }

    public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt;
    public bool IsActive(DateTimeOffset now) => !IsExpired(now);
}
