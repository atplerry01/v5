namespace Whycespace.Domain.BusinessSystem.Subscription.Plan;

public sealed class PlanAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public PlanId Id { get; private set; }
    public PlanDescriptor Descriptor { get; private set; }
    public PlanStatus Status { get; private set; }
    public int Version { get; private set; }

    private PlanAggregate() { }

    // -- Factory ----------------------------------------------------------

    public static PlanAggregate Draft(
        PlanId id,
        PlanDescriptor descriptor)
    {
        var aggregate = new PlanAggregate();

        var @event = new PlanDraftedEvent(id, descriptor);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    // -- Activate ---------------------------------------------------------

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PlanErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new PlanActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    // -- Deprecate --------------------------------------------------------

    public void Deprecate()
    {
        var specification = new CanDeprecateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PlanErrors.InvalidStateTransition(Status, nameof(Deprecate));

        var @event = new PlanDeprecatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    // -- Apply ------------------------------------------------------------

    private void Apply(PlanDraftedEvent @event)
    {
        Id = @event.PlanId;
        Descriptor = @event.Descriptor;
        Status = PlanStatus.Draft;
        Version++;
    }

    private void Apply(PlanActivatedEvent @event)
    {
        Status = PlanStatus.Active;
        Version++;
    }

    private void Apply(PlanDeprecatedEvent @event)
    {
        Status = PlanStatus.Deprecated;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw PlanErrors.MissingId();

        if (Descriptor == default)
            throw PlanErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw PlanErrors.InvalidStateTransition(Status, "validate");
    }
}
