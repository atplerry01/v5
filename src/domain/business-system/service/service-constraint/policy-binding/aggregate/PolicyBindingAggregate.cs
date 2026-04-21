using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.PolicyBinding;

public sealed class PolicyBindingAggregate : AggregateRoot
{
    public PolicyBindingId Id { get; private set; }
    public ServiceDefinitionRef ServiceDefinition { get; private set; }
    public PolicyRef Policy { get; private set; }
    public PolicyBindingScope Scope { get; private set; }
    public PolicyBindingStatus Status { get; private set; }

    public static PolicyBindingAggregate Create(
        PolicyBindingId id,
        ServiceDefinitionRef serviceDefinition,
        PolicyRef policy,
        PolicyBindingScope scope)
    {
        var aggregate = new PolicyBindingAggregate();
        if (aggregate.Version >= 0)
            throw PolicyBindingErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new PolicyBindingCreatedEvent(id, serviceDefinition, policy, scope));
        return aggregate;
    }

    public void Bind(DateTimeOffset boundAt)
    {
        var specification = new CanBindSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PolicyBindingErrors.InvalidStateTransition(Status, nameof(Bind));

        RaiseDomainEvent(new PolicyBindingBoundEvent(Id, boundAt));
    }

    public void Unbind(DateTimeOffset unboundAt)
    {
        var specification = new CanUnbindSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PolicyBindingErrors.InvalidStateTransition(Status, nameof(Unbind));

        RaiseDomainEvent(new PolicyBindingUnboundEvent(Id, unboundAt));
    }

    public void Archive()
    {
        if (Status == PolicyBindingStatus.Archived)
            throw PolicyBindingErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new PolicyBindingArchivedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case PolicyBindingCreatedEvent e:
                Id = e.PolicyBindingId;
                ServiceDefinition = e.ServiceDefinition;
                Policy = e.Policy;
                Scope = e.Scope;
                Status = PolicyBindingStatus.Draft;
                break;
            case PolicyBindingBoundEvent:
                Status = PolicyBindingStatus.Bound;
                break;
            case PolicyBindingUnboundEvent:
                Status = PolicyBindingStatus.Unbound;
                break;
            case PolicyBindingArchivedEvent:
                Status = PolicyBindingStatus.Archived;
                break;
        }
    }

    protected override void EnsureInvariants()
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
