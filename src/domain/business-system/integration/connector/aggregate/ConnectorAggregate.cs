namespace Whycespace.Domain.BusinessSystem.Integration.Connector;

public sealed class ConnectorAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ConnectorId Id { get; private set; }
    public ConnectorTargetId TargetId { get; private set; }
    public ConnectorStatus Status { get; private set; }
    public int Version { get; private set; }

    private ConnectorAggregate() { }

    public static ConnectorAggregate Create(ConnectorId id, ConnectorTargetId targetId)
    {
        var aggregate = new ConnectorAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new ConnectorCreatedEvent(id, targetId);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Connect()
    {
        ValidateBeforeChange();

        var specification = new CanConnectSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ConnectorErrors.InvalidStateTransition(Status, nameof(Connect));

        var @event = new ConnectorConnectedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Disconnect()
    {
        ValidateBeforeChange();

        var specification = new CanDisconnectSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ConnectorErrors.InvalidStateTransition(Status, nameof(Disconnect));

        var @event = new ConnectorDisconnectedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ConnectorCreatedEvent @event)
    {
        Id = @event.ConnectorId;
        TargetId = @event.TargetId;
        Status = ConnectorStatus.Defined;
        Version++;
    }

    private void Apply(ConnectorConnectedEvent @event)
    {
        Status = ConnectorStatus.Connected;
        Version++;
    }

    private void Apply(ConnectorDisconnectedEvent @event)
    {
        Status = ConnectorStatus.Disconnected;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw ConnectorErrors.MissingId();

        if (TargetId == default)
            throw ConnectorErrors.MissingTargetId();

        if (!Enum.IsDefined(Status))
            throw ConnectorErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
