using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderCapability;

public sealed class ProviderCapabilityAggregate : AggregateRoot
{
    public ProviderCapabilityId Id { get; private set; }
    public ClusterProviderRef Provider { get; private set; }
    public CapabilityCode Code { get; private set; }
    public CapabilityName Name { get; private set; }
    public CapabilityStatus Status { get; private set; }

    public static ProviderCapabilityAggregate Create(
        ProviderCapabilityId id,
        ClusterProviderRef provider,
        CapabilityCode code,
        CapabilityName name)
    {
        var aggregate = new ProviderCapabilityAggregate();
        if (aggregate.Version >= 0)
            throw ProviderCapabilityErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new ProviderCapabilityCreatedEvent(id, provider, code, name));
        return aggregate;
    }

    public void Update(CapabilityName name)
    {
        EnsureMutable();
        RaiseDomainEvent(new ProviderCapabilityUpdatedEvent(Id, name));
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProviderCapabilityErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new ProviderCapabilityActivatedEvent(Id));
    }

    public void Archive()
    {
        if (Status == CapabilityStatus.Archived)
            throw ProviderCapabilityErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new ProviderCapabilityArchivedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ProviderCapabilityCreatedEvent e:
                Id = e.ProviderCapabilityId;
                Provider = e.Provider;
                Code = e.Code;
                Name = e.Name;
                Status = CapabilityStatus.Draft;
                break;
            case ProviderCapabilityUpdatedEvent e:
                Name = e.Name;
                break;
            case ProviderCapabilityActivatedEvent:
                Status = CapabilityStatus.Active;
                break;
            case ProviderCapabilityArchivedEvent:
                Status = CapabilityStatus.Archived;
                break;
        }
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProviderCapabilityErrors.ArchivedImmutable(Id);
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw ProviderCapabilityErrors.MissingId();

        if (Provider == default)
            throw ProviderCapabilityErrors.MissingProviderRef();

        if (!Enum.IsDefined(Status))
            throw ProviderCapabilityErrors.InvalidStateTransition(Status, "validate");
    }
}
