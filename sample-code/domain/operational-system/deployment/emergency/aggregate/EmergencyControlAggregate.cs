using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OperationalSystem.Deployment.Emergency;

/// <summary>
/// Emergency control authority. Provides system-wide, region-level,
/// and SPV-level halt capabilities for immediate risk containment.
/// </summary>
public sealed class EmergencyControlAggregate : AggregateRoot
{
    public EmergencyScope Scope { get; private set; } = null!;
    public EmergencyStatus Status { get; private set; } = EmergencyStatus.Standby;
    public string? HaltReason { get; private set; }
    public string? InitiatedBy { get; private set; }

    public static EmergencyControlAggregate Create(Guid id, EmergencyScope scope)
    {
        var agg = new EmergencyControlAggregate
        {
            Id = id,
            Scope = scope,
            Status = EmergencyStatus.Standby
        };
        agg.RaiseDomainEvent(new EmergencyControlCreatedEvent(id, scope.ScopeType, scope.TargetId));
        return agg;
    }

    public void ActivateHalt(string reason, string initiatedBy)
    {
        EnsureInvariant(Status == EmergencyStatus.Standby || Status == EmergencyStatus.Resolved,
            "MustBeStandbyOrResolved", $"Cannot halt from status {Status.Value}.");
        Status = EmergencyStatus.Active;
        HaltReason = reason;
        InitiatedBy = initiatedBy;
        RaiseDomainEvent(new EmergencyHaltActivatedEvent(Id, Scope.ScopeType, Scope.TargetId, reason, initiatedBy));
    }

    public void Resolve(string resolvedBy, string resolution)
    {
        EnsureInvariant(Status == EmergencyStatus.Active,
            "MustBeActive", "Cannot resolve a halt that is not active.");
        Status = EmergencyStatus.Resolved;
        RaiseDomainEvent(new EmergencyHaltResolvedEvent(Id, resolvedBy, resolution));
    }
}
