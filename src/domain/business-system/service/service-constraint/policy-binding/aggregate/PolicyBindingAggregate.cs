using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.PolicyBinding;

public sealed class PolicyBindingAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public PolicyBindingId Id { get; private set; }
    public ServiceDefinitionRef ServiceDefinition { get; private set; }
    public PolicyRef Policy { get; private set; }
    public PolicyBindingScope Scope { get; private set; }
    public PolicyBindingStatus Status { get; private set; }
    public int Version { get; private set; }

    private PolicyBindingAggregate() { }

    public static PolicyBindingAggregate Create(
        PolicyBindingId id,
        ServiceDefinitionRef serviceDefinition,
        PolicyRef policy,
        PolicyBindingScope scope)
    {
        var aggregate = new PolicyBindingAggregate();

        var @event = new PolicyBindingCreatedEvent(id, serviceDefinition, policy, scope);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Bind(DateTimeOffset boundAt)
    {
        var specification = new CanBindSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PolicyBindingErrors.InvalidStateTransition(Status, nameof(Bind));

        var @event = new PolicyBindingBoundEvent(Id, boundAt);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Unbind(DateTimeOffset unboundAt)
    {
        var specification = new CanUnbindSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PolicyBindingErrors.InvalidStateTransition(Status, nameof(Unbind));

        var @event = new PolicyBindingUnboundEvent(Id, unboundAt);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        if (Status == PolicyBindingStatus.Archived)
            throw PolicyBindingErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new PolicyBindingArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(PolicyBindingCreatedEvent @event)
    {
        Id = @event.PolicyBindingId;
        ServiceDefinition = @event.ServiceDefinition;
        Policy = @event.Policy;
        Scope = @event.Scope;
        Status = PolicyBindingStatus.Draft;
        Version++;
    }

    private void Apply(PolicyBindingBoundEvent @event)
    {
        Status = PolicyBindingStatus.Bound;
        Version++;
    }

    private void Apply(PolicyBindingUnboundEvent @event)
    {
        Status = PolicyBindingStatus.Unbound;
        Version++;
    }

    private void Apply(PolicyBindingArchivedEvent @event)
    {
        Status = PolicyBindingStatus.Archived;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw PolicyBindingErrors.MissingId();

        if (ServiceDefinition == default)
            throw PolicyBindingErrors.MissingServiceDefinitionRef();

        if (!Enum.IsDefined(Status))
            throw PolicyBindingErrors.InvalidStateTransition(Status, "validate");

        if (!Enum.IsDefined(Scope))
            throw PolicyBindingErrors.InvalidStateTransition(Status, "validate-scope");
    }
}
