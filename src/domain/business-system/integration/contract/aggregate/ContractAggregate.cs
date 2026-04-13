namespace Whycespace.Domain.BusinessSystem.Integration.Contract;

public sealed class ContractAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ContractId Id { get; private set; }
    public ContractSchema Schema { get; private set; } = null!;
    public ContractStatus Status { get; private set; }
    public int Version { get; private set; }

    private ContractAggregate() { }

    public static ContractAggregate Create(ContractId id, ContractSchema schema)
    {
        if (schema is null)
            throw new ArgumentNullException(nameof(schema));

        var aggregate = new ContractAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new ContractCreatedEvent(id, schema.SchemaId, schema.SchemaName);
        aggregate.Apply(@event, schema);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Activate()
    {
        ValidateBeforeChange();

        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ContractErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new ContractActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Terminate()
    {
        ValidateBeforeChange();

        var specification = new CanTerminateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ContractErrors.InvalidStateTransition(Status, nameof(Terminate));

        var @event = new ContractTerminatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ContractCreatedEvent @event, ContractSchema schema)
    {
        Id = @event.ContractId;
        Schema = schema;
        Status = ContractStatus.Draft;
        Version++;
    }

    private void Apply(ContractActivatedEvent @event)
    {
        Status = ContractStatus.Active;
        Version++;
    }

    private void Apply(ContractTerminatedEvent @event)
    {
        Status = ContractStatus.Terminated;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw ContractErrors.MissingId();

        if (Schema is null)
            throw ContractErrors.MissingSchema();

        if (!Enum.IsDefined(Status))
            throw ContractErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
