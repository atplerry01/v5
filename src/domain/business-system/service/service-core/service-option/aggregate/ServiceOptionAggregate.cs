using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceOption;

public sealed class ServiceOptionAggregate : AggregateRoot
{
    public ServiceOptionId Id { get; private set; }
    public ServiceDefinitionRef ServiceDefinition { get; private set; }
    public OptionCode Code { get; private set; }
    public OptionName Name { get; private set; }
    public OptionKind Kind { get; private set; }
    public ServiceOptionStatus Status { get; private set; }

    public static ServiceOptionAggregate Create(
        ServiceOptionId id,
        ServiceDefinitionRef serviceDefinition,
        OptionCode code,
        OptionName name,
        OptionKind kind)
    {
        var aggregate = new ServiceOptionAggregate();
        if (aggregate.Version >= 0)
            throw ServiceOptionErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new ServiceOptionCreatedEvent(id, serviceDefinition, code, name, kind));
        return aggregate;
    }

    public void Update(OptionName name, OptionKind kind)
    {
        EnsureMutable();

        RaiseDomainEvent(new ServiceOptionUpdatedEvent(Id, name, kind));
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ServiceOptionErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new ServiceOptionActivatedEvent(Id));
    }

    public void Archive()
    {
        if (Status == ServiceOptionStatus.Archived)
            throw ServiceOptionErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new ServiceOptionArchivedEvent(Id));
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ServiceOptionErrors.ArchivedImmutable(Id);
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ServiceOptionCreatedEvent e:
                Id = e.ServiceOptionId;
                ServiceDefinition = e.ServiceDefinition;
                Code = e.Code;
                Name = e.Name;
                Kind = e.Kind;
                Status = ServiceOptionStatus.Draft;
                break;
            case ServiceOptionUpdatedEvent e:
                Name = e.Name;
                Kind = e.Kind;
                break;
            case ServiceOptionActivatedEvent:
                Status = ServiceOptionStatus.Active;
                break;
            case ServiceOptionArchivedEvent:
                Status = ServiceOptionStatus.Archived;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw ServiceOptionErrors.MissingId();

        if (ServiceDefinition == default)
            throw ServiceOptionErrors.MissingServiceDefinitionRef();

        if (!Enum.IsDefined(Status))
            throw ServiceOptionErrors.InvalidStateTransition(Status, "validate");
    }
}
