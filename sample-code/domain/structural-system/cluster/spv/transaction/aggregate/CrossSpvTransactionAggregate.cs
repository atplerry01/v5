using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Spv.Transaction;

public sealed class CrossSpvTransactionAggregate : AggregateRoot
{
    public Guid RootSpvId { get; private set; }
    public IReadOnlyList<SpvLeg> Legs => _legs;
    public CrossSpvStatus Status { get; private set; } = CrossSpvStatus.Created;
    public CrossSpvExecutionState ExecutionState { get; private set; } = CrossSpvExecutionState.Pending;

    private readonly List<SpvLeg> _legs = new();
    private readonly HashSet<Guid> _processed = new();

    public CrossSpvTransactionAggregate() { }

    public static CrossSpvTransactionAggregate Create(
        Guid id,
        Guid rootSpvId,
        IReadOnlyList<SpvLeg> legs)
    {
        if (id == Guid.Empty)
            throw new CrossSpvException("Transaction id required");

        if (rootSpvId == Guid.Empty)
            throw new CrossSpvException("Root SPV required");

        if (legs == null || legs.Count == 0)
            throw new CrossSpvException("At least one leg required");

        CrossSpvInvariantService.Validate(legs);

        var aggregate = new CrossSpvTransactionAggregate
        {
            Id = id,
            RootSpvId = rootSpvId,
            Status = CrossSpvStatus.Created,
            ExecutionState = CrossSpvExecutionState.Pending
        };

        aggregate._legs.AddRange(legs);

        aggregate.RaiseDomainEvent(new CrossSpvCreatedEvent(id, rootSpvId));

        return aggregate;
    }

    public void SetExecutionState(CrossSpvExecutionState state)
    {
        ExecutionState = state;

        RaiseDomainEvent(new CrossSpvStateChangedEvent(Id, state.Value));
    }

    public void Prepare(Guid transactionId)
    {
        EnsureIdempotent(transactionId);

        if (Status != CrossSpvStatus.Created)
            throw new CrossSpvException("Invalid state: only Created transactions can be prepared");

        Status = CrossSpvStatus.Prepared;

        RaiseDomainEvent(new CrossSpvPreparedEvent(Id, transactionId));
    }

    public void Commit(Guid transactionId)
    {
        EnsureIdempotent(transactionId);

        if (Status != CrossSpvStatus.Prepared)
            throw new CrossSpvException("Not prepared: must prepare before commit");

        Status = CrossSpvStatus.Committed;
        ExecutionState = CrossSpvExecutionState.Completed;

        RaiseDomainEvent(new CrossSpvCommittedEvent(Id, transactionId));
    }

    public void Fail(Guid transactionId, string reason)
    {
        EnsureIdempotent(transactionId);

        if (Status == CrossSpvStatus.Committed)
            throw new CrossSpvException("Cannot fail a committed transaction");

        if (Status == CrossSpvStatus.Failed)
            throw new CrossSpvException("Transaction already failed");

        Status = CrossSpvStatus.Failed;
        ExecutionState = CrossSpvExecutionState.Failed;

        RaiseDomainEvent(new CrossSpvFailedEvent(Id, transactionId, reason));
    }

    private void EnsureIdempotent(Guid transactionId)
    {
        if (transactionId == Guid.Empty)
            throw new CrossSpvException("Transaction id required");

        if (!_processed.Add(transactionId))
            throw new CrossSpvException("Duplicate transaction");
    }
}
