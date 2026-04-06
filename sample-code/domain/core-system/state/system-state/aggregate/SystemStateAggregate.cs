using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.State.SystemState;

/// <summary>
/// Single source of truth for system state. Manages:
/// - System snapshots (point-in-time state captures)
/// - Cross-system state validation
/// - Authoritative state declaration
/// </summary>
public sealed class SystemStateAggregate : AggregateRoot
{
    public SystemStateStatus Status { get; private set; } = SystemStateStatus.Initializing;
    public SystemSnapshot? CurrentSnapshot { get; private set; }
    private readonly List<StateValidationResult> _validations = [];
    public IReadOnlyList<StateValidationResult> Validations => _validations.AsReadOnly();

    public static SystemStateAggregate Initialize(Guid id)
    {
        var agg = new SystemStateAggregate { Id = id };
        agg.RaiseDomainEvent(new SystemStateInitializedEvent(id));
        return agg;
    }

    public void Activate()
    {
        EnsureValidTransition(Status, SystemStateStatus.Active, SystemStateStatus.IsValidTransition);
        Status = SystemStateStatus.Active;
        RaiseDomainEvent(new SystemStateActivatedEvent(Id));
    }

    public void CaptureSnapshot(SystemSnapshot snapshot)
    {
        EnsureInvariant(
            Status == SystemStateStatus.Active || Status == SystemStateStatus.Validating,
            "ActiveOrValidating",
            $"Cannot capture snapshot in status: {Status.Value}");
        CurrentSnapshot = snapshot;
        Status = SystemStateStatus.Validating;
        RaiseDomainEvent(new SystemSnapshotCapturedEvent(
            Id, snapshot.EventStoreVersion, snapshot.ActiveAggregates, snapshot.SnapshotHash));
    }

    public void RecordValidation(StateValidationResult result)
    {
        EnsureInvariant(
            Status == SystemStateStatus.Validating,
            "MustBeValidating",
            $"Cannot record validation in status: {Status.Value}");
        _validations.Add(result);
        RaiseDomainEvent(new StateValidationRecordedEvent(Id, result.SystemName, result.IsValid, result.Details));
    }

    public void DeclareAuthoritative()
    {
        EnsureInvariant(
            Status == SystemStateStatus.Validating,
            "MustBeValidating",
            $"Cannot declare authoritative in status: {Status.Value}");
        EnsureInvariant(
            CurrentSnapshot is not null,
            "SnapshotRequired",
            "Cannot declare authoritative state without a captured snapshot.");
        EnsureInvariant(
            _validations.Count > 0 && _validations.All(v => v.IsValid),
            "AllValidationsMustPass",
            "Cannot declare authoritative state with failed validations.");
        Status = SystemStateStatus.Authoritative;
        RaiseDomainEvent(new SystemStateDeclaredAuthoritativeEvent(Id, CurrentSnapshot!.SnapshotHash));
    }

    public void DeclareDegraded(string reason)
    {
        EnsureInvariant(
            Status == SystemStateStatus.Validating,
            "MustBeValidating",
            $"Cannot declare degraded in status: {Status.Value}");
        Status = SystemStateStatus.Degraded;
        RaiseDomainEvent(new SystemStateDegradedEvent(Id, reason));
    }
}
