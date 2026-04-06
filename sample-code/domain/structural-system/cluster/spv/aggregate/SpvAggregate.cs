using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

public sealed class SpvAggregate : AggregateRoot
{
    public Guid SubClusterId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public SpvStatus Status { get; private set; } = SpvStatus.Created;
    public DateTimeOffset? TerminatedAt { get; private set; }
    public string? TerminationReason { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }
    public Guid? AuditRecordId { get; private set; }

    private readonly HashSet<Guid> _operators = new();
    public IReadOnlyCollection<Guid> Operators => _operators;

    public SpvAggregate() { }

    public static SpvAggregate Create(
        Guid id,
        Guid subClusterId,
        string name)
    {
        if (id == Guid.Empty)
            throw new SpvException("SPV id required");

        if (subClusterId == Guid.Empty)
            throw new SpvException("SubCluster required");

        if (string.IsNullOrWhiteSpace(name))
            throw new SpvException("SPV name required");

        var aggregate = new SpvAggregate
        {
            Id = id,
            SubClusterId = subClusterId,
            Name = name,
            Status = SpvStatus.Created
        };

        aggregate.RaiseDomainEvent(new SpvCreatedEvent(id));

        return aggregate;
    }

    public void Activate()
    {
        EnsureValidTransition(Status, SpvStatus.Active,
            (from, to) => from.CanTransitionTo(to));

        Status = SpvStatus.Active;
        RaiseDomainEvent(new SpvActivatedEvent(Id));
    }

    public void Suspend(string reason)
    {
        EnsureValidTransition(Status, SpvStatus.Suspended,
            (from, to) => from.CanTransitionTo(to));

        Status = SpvStatus.Suspended;
        RaiseDomainEvent(new SpvSuspendedEvent(Id, reason));
    }

    public void Reactivate()
    {
        EnsureValidTransition(Status, SpvStatus.Active,
            (from, to) => from.CanTransitionTo(to));

        Status = SpvStatus.Active;
        RaiseDomainEvent(new SpvReactivatedEvent(Id));
    }

    public void Terminate(string reason, DateTimeOffset timestamp)
    {
        EnsureValidTransition(Status, SpvStatus.Terminated,
            (from, to) => from.CanTransitionTo(to));

        Status = SpvStatus.Terminated;
        TerminatedAt = timestamp;
        TerminationReason = reason;
        RaiseDomainEvent(new SpvTerminatedEvent(Id, reason));
    }

    public void Close(Guid auditRecordId, DateTimeOffset timestamp)
    {
        EnsureValidTransition(Status, SpvStatus.Closed,
            (from, to) => from.CanTransitionTo(to));

        Guard.AgainstDefault(auditRecordId);

        Status = SpvStatus.Closed;
        ClosedAt = timestamp;
        AuditRecordId = auditRecordId;
        RaiseDomainEvent(new SpvClosedEvent(Id, auditRecordId));
    }

    public void AddOperator(Guid identityId)
    {
        if (identityId == Guid.Empty)
            throw new SpvException("Operator identity id required");

        EnsureInvariant(
            Status == SpvStatus.Active,
            "SPV_MUST_BE_ACTIVE",
            "Only active SPVs can add operators.");

        if (!_operators.Add(identityId))
            throw new SpvException("Operator already exists in this SPV");

        RaiseDomainEvent(new SpvOperatorAddedEvent(Id, identityId));
    }

    public void ReplaceOperator(Guid oldId, Guid newId)
    {
        if (oldId == Guid.Empty || newId == Guid.Empty)
            throw new SpvException("Operator identity ids required");

        EnsureInvariant(
            Status == SpvStatus.Active || Status == SpvStatus.Suspended,
            "SPV_MUST_BE_OPERATIONAL",
            "Only active or suspended SPVs can replace operators.");

        if (!_operators.Contains(oldId))
            throw new SpvException("Operator not found");

        if (_operators.Contains(newId))
            throw new SpvException("Replacement operator already exists");

        _operators.Remove(oldId);
        _operators.Add(newId);

        RaiseDomainEvent(new SpvOperatorReplacedEvent(Id, oldId, newId));
    }
}
