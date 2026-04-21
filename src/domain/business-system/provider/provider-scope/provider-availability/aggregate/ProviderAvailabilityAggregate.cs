using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

using Whycespace.Domain.BusinessSystem.Shared.Time;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderAvailability;

public sealed class ProviderAvailabilityAggregate : AggregateRoot
{
    public ProviderAvailabilityId Id { get; private set; }
    public ClusterProviderRef Provider { get; private set; }
    public TimeWindow Window { get; private set; }
    public ProviderAvailabilityStatus Status { get; private set; }

    public static ProviderAvailabilityAggregate Create(
        ProviderAvailabilityId id,
        ClusterProviderRef provider,
        TimeWindow window)
    {
        var aggregate = new ProviderAvailabilityAggregate();
        if (aggregate.Version >= 0)
            throw ProviderAvailabilityErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new ProviderAvailabilityCreatedEvent(id, provider, window));
        return aggregate;
    }

    public void UpdateWindow(TimeWindow window)
    {
        EnsureMutable();
        RaiseDomainEvent(new ProviderAvailabilityUpdatedEvent(Id, window));
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProviderAvailabilityErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new ProviderAvailabilityActivatedEvent(Id));
    }

    public void Archive()
    {
        if (Status == ProviderAvailabilityStatus.Archived)
            throw ProviderAvailabilityErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new ProviderAvailabilityArchivedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ProviderAvailabilityCreatedEvent e:
                Id = e.ProviderAvailabilityId;
                Provider = e.Provider;
                Window = e.Window;
                Status = ProviderAvailabilityStatus.Draft;
                break;
            case ProviderAvailabilityUpdatedEvent e:
                Window = e.Window;
                break;
            case ProviderAvailabilityActivatedEvent:
                Status = ProviderAvailabilityStatus.Active;
                break;
            case ProviderAvailabilityArchivedEvent:
                Status = ProviderAvailabilityStatus.Archived;
                break;
        }
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProviderAvailabilityErrors.ArchivedImmutable(Id);
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw ProviderAvailabilityErrors.MissingId();

        if (Provider == default)
            throw ProviderAvailabilityErrors.MissingProviderRef();

        if (!Enum.IsDefined(Status))
            throw ProviderAvailabilityErrors.InvalidStateTransition(Status, "validate");
    }
}
