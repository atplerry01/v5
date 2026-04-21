using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceLevel;

public sealed class ServiceLevelAggregate : AggregateRoot
{
    public ServiceLevelId Id { get; private set; }
    public ServiceDefinitionRef ServiceDefinition { get; private set; }
    public LevelCode Code { get; private set; }
    public LevelName Name { get; private set; }
    public ServiceLevelTarget Target { get; private set; }
    public ServiceLevelStatus Status { get; private set; }

    public static ServiceLevelAggregate Create(
        ServiceLevelId id,
        ServiceDefinitionRef serviceDefinition,
        LevelCode code,
        LevelName name,
        ServiceLevelTarget target)
    {
        var aggregate = new ServiceLevelAggregate();
        if (aggregate.Version >= 0)
            throw ServiceLevelErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new ServiceLevelCreatedEvent(id, serviceDefinition, code, name, target));
        return aggregate;
    }

    public void Update(LevelName name, ServiceLevelTarget target)
    {
        EnsureMutable();

        RaiseDomainEvent(new ServiceLevelUpdatedEvent(Id, name, target));
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ServiceLevelErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new ServiceLevelActivatedEvent(Id));
    }

    public void Archive()
    {
        if (Status == ServiceLevelStatus.Archived)
            throw ServiceLevelErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new ServiceLevelArchivedEvent(Id));
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ServiceLevelErrors.ArchivedImmutable(Id);
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ServiceLevelCreatedEvent e:
                Id = e.ServiceLevelId;
                ServiceDefinition = e.ServiceDefinition;
                Code = e.Code;
                Name = e.Name;
                Target = e.Target;
                Status = ServiceLevelStatus.Draft;
                break;
            case ServiceLevelUpdatedEvent e:
                Name = e.Name;
                Target = e.Target;
                break;
            case ServiceLevelActivatedEvent:
                Status = ServiceLevelStatus.Active;
                break;
            case ServiceLevelArchivedEvent:
                Status = ServiceLevelStatus.Archived;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw ServiceLevelErrors.MissingId();

        if (ServiceDefinition == default)
            throw ServiceLevelErrors.MissingServiceDefinitionRef();

        if (!Enum.IsDefined(Status))
            throw ServiceLevelErrors.InvalidStateTransition(Status, "validate");
    }
}
