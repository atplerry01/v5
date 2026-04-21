using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceConstraint;

public sealed class ServiceConstraintAggregate : AggregateRoot
{
    public ServiceConstraintId Id { get; private set; }
    public ServiceDefinitionRef ServiceDefinition { get; private set; }
    public ConstraintKind Kind { get; private set; }
    public ConstraintDescriptor Descriptor { get; private set; }
    public ConstraintStatus Status { get; private set; }

    public static ServiceConstraintAggregate Create(
        ServiceConstraintId id,
        ServiceDefinitionRef serviceDefinition,
        ConstraintKind kind,
        ConstraintDescriptor descriptor)
    {
        var aggregate = new ServiceConstraintAggregate();
        if (aggregate.Version >= 0)
            throw ServiceConstraintErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new ServiceConstraintCreatedEvent(id, serviceDefinition, kind, descriptor));
        return aggregate;
    }

    public void Update(ConstraintKind kind, ConstraintDescriptor descriptor)
    {
        EnsureMutable();

        RaiseDomainEvent(new ServiceConstraintUpdatedEvent(Id, kind, descriptor));
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ServiceConstraintErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new ServiceConstraintActivatedEvent(Id));
    }

    public void Archive()
    {
        if (Status == ConstraintStatus.Archived)
            throw ServiceConstraintErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new ServiceConstraintArchivedEvent(Id));
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ServiceConstraintErrors.ArchivedImmutable(Id);
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ServiceConstraintCreatedEvent e:
                Id = e.ServiceConstraintId;
                ServiceDefinition = e.ServiceDefinition;
                Kind = e.Kind;
                Descriptor = e.Descriptor;
                Status = ConstraintStatus.Draft;
                break;
            case ServiceConstraintUpdatedEvent e:
                Kind = e.Kind;
                Descriptor = e.Descriptor;
                break;
            case ServiceConstraintActivatedEvent:
                Status = ConstraintStatus.Active;
                break;
            case ServiceConstraintArchivedEvent:
                Status = ConstraintStatus.Archived;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw ServiceConstraintErrors.MissingId();

        if (ServiceDefinition == default)
            throw ServiceConstraintErrors.MissingServiceDefinitionRef();

        if (!Enum.IsDefined(Status))
            throw ServiceConstraintErrors.InvalidStateTransition(Status, "validate");
    }
}
