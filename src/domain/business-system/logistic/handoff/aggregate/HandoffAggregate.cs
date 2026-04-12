namespace Whycespace.Domain.BusinessSystem.Logistic.Handoff;

public sealed class HandoffAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public HandoffId Id { get; private set; }
    public ActorReference SourceActor { get; private set; }
    public ActorReference TargetActor { get; private set; }
    public TransferReference TransferReference { get; private set; }
    public HandoffStatus Status { get; private set; }
    public int Version { get; private set; }

    private HandoffAggregate() { }

    public static HandoffAggregate Create(
        HandoffId id,
        ActorReference sourceActor,
        ActorReference targetActor,
        TransferReference transferReference)
    {
        var specification = new HandoffSpecification();
        if (!specification.IsSatisfiedBy(id, sourceActor, targetActor, transferReference))
        {
            if (id == default)
                throw HandoffErrors.MissingId();
            if (sourceActor == default)
                throw HandoffErrors.MissingSourceActor();
            if (targetActor == default)
                throw HandoffErrors.MissingTargetActor();
            if (sourceActor == targetActor)
                throw HandoffErrors.ActorsCannotBeEqual(sourceActor);
            if (transferReference == default)
                throw HandoffErrors.MissingTransferReference();
        }

        var aggregate = new HandoffAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new HandoffCreatedEvent(id, sourceActor, targetActor, transferReference);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Transfer()
    {
        if (Status == HandoffStatus.Transferred)
            throw HandoffErrors.AlreadyTransferred();

        ValidateBeforeChange();

        var transferredEvent = new HandoffTransferredEvent(Id);
        Apply(transferredEvent);
        AddEvent(transferredEvent);

        EnsureInvariants();
    }

    private void Apply(HandoffCreatedEvent @event)
    {
        Id = @event.HandoffId;
        SourceActor = @event.SourceActor;
        TargetActor = @event.TargetActor;
        TransferReference = @event.TransferReference;
        Status = HandoffStatus.Created;
        Version++;
    }

    private void Apply(HandoffTransferredEvent @event)
    {
        Status = HandoffStatus.Transferred;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw HandoffErrors.MissingId();

        if (SourceActor == default)
            throw HandoffErrors.MissingSourceActor();

        if (TargetActor == default)
            throw HandoffErrors.MissingTargetActor();

        if (SourceActor == TargetActor)
            throw HandoffErrors.ActorsCannotBeEqual(SourceActor);

        if (TransferReference == default)
            throw HandoffErrors.MissingTransferReference();

        if (!Enum.IsDefined(Status))
            throw new InvalidOperationException("HandoffStatus is not a defined enum value.");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
