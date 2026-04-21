using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderTier;

public sealed class ProviderTierAggregate : AggregateRoot
{
    public ProviderTierId Id { get; private set; }
    public TierCode Code { get; private set; }
    public TierName Name { get; private set; }
    public TierRank Rank { get; private set; }
    public ProviderTierStatus Status { get; private set; }

    public static ProviderTierAggregate Create(
        ProviderTierId id,
        TierCode code,
        TierName name,
        TierRank rank)
    {
        var aggregate = new ProviderTierAggregate();
        if (aggregate.Version >= 0)
            throw ProviderTierErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new ProviderTierCreatedEvent(id, code, name, rank));
        return aggregate;
    }

    public void Update(TierName name, TierRank rank)
    {
        EnsureMutable();
        RaiseDomainEvent(new ProviderTierUpdatedEvent(Id, name, rank));
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProviderTierErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new ProviderTierActivatedEvent(Id));
    }

    public void Archive()
    {
        if (Status == ProviderTierStatus.Archived)
            throw ProviderTierErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new ProviderTierArchivedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ProviderTierCreatedEvent e:
                Id = e.ProviderTierId;
                Code = e.Code;
                Name = e.Name;
                Rank = e.Rank;
                Status = ProviderTierStatus.Draft;
                break;
            case ProviderTierUpdatedEvent e:
                Name = e.Name;
                Rank = e.Rank;
                break;
            case ProviderTierActivatedEvent:
                Status = ProviderTierStatus.Active;
                break;
            case ProviderTierArchivedEvent:
                Status = ProviderTierStatus.Archived;
                break;
        }
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProviderTierErrors.ArchivedImmutable(Id);
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw ProviderTierErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw ProviderTierErrors.InvalidStateTransition(Status, "validate");
    }
}
