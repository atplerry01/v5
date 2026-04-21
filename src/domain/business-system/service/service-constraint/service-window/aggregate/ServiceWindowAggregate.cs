using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.BusinessSystem.Shared.Time;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceWindow;

public sealed class ServiceWindowAggregate : AggregateRoot
{
    public ServiceWindowId Id { get; private set; }
    public ServiceDefinitionRef ServiceDefinition { get; private set; }
    public TimeWindow Range { get; private set; }
    public ServiceWindowStatus Status { get; private set; }

    public static ServiceWindowAggregate Create(
        ServiceWindowId id,
        ServiceDefinitionRef serviceDefinition,
        TimeWindow range)
    {
        var aggregate = new ServiceWindowAggregate();
        if (aggregate.Version >= 0)
            throw ServiceWindowErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new ServiceWindowCreatedEvent(id, serviceDefinition, range));
        return aggregate;
    }

    public void UpdateRange(TimeWindow range)
    {
        EnsureMutable();

        RaiseDomainEvent(new ServiceWindowUpdatedEvent(Id, range));
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ServiceWindowErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new ServiceWindowActivatedEvent(Id));
    }

    public void Archive()
    {
        if (Status == ServiceWindowStatus.Archived)
            throw ServiceWindowErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new ServiceWindowArchivedEvent(Id));
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ServiceWindowErrors.ArchivedImmutable(Id);
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ServiceWindowCreatedEvent e:
                Id = e.ServiceWindowId;
                ServiceDefinition = e.ServiceDefinition;
                Range = e.Range;
                Status = ServiceWindowStatus.Draft;
                break;
            case ServiceWindowUpdatedEvent e:
                Range = e.Range;
                break;
            case ServiceWindowActivatedEvent:
                Status = ServiceWindowStatus.Active;
                break;
            case ServiceWindowArchivedEvent:
                Status = ServiceWindowStatus.Archived;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw ServiceWindowErrors.MissingId();

        if (ServiceDefinition == default)
            throw ServiceWindowErrors.MissingServiceDefinitionRef();

        if (!Range.IsClosed)
            throw ServiceWindowErrors.ClosedWindowRequired();

        if (!Enum.IsDefined(Status))
            throw ServiceWindowErrors.InvalidStateTransition(Status, "validate");
    }
}
