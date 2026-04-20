namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderTier;

public sealed class ProviderTierAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ProviderTierId Id { get; private set; }
    public TierCode Code { get; private set; }
    public TierName Name { get; private set; }
    public TierRank Rank { get; private set; }
    public ProviderTierStatus Status { get; private set; }
    public int Version { get; private set; }

    private ProviderTierAggregate() { }

    public static ProviderTierAggregate Create(
        ProviderTierId id,
        TierCode code,
        TierName name,
        TierRank rank)
    {
        var aggregate = new ProviderTierAggregate();

        var @event = new ProviderTierCreatedEvent(id, code, name, rank);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Update(TierName name, TierRank rank)
    {
        EnsureMutable();

        var @event = new ProviderTierUpdatedEvent(Id, name, rank);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProviderTierErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new ProviderTierActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        if (Status == ProviderTierStatus.Archived)
            throw ProviderTierErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new ProviderTierArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ProviderTierCreatedEvent @event)
    {
        Id = @event.ProviderTierId;
        Code = @event.Code;
        Name = @event.Name;
        Rank = @event.Rank;
        Status = ProviderTierStatus.Draft;
        Version++;
    }

    private void Apply(ProviderTierUpdatedEvent @event)
    {
        Name = @event.Name;
        Rank = @event.Rank;
        Version++;
    }

    private void Apply(ProviderTierActivatedEvent @event)
    {
        Status = ProviderTierStatus.Active;
        Version++;
    }

    private void Apply(ProviderTierArchivedEvent @event)
    {
        Status = ProviderTierStatus.Archived;
        Version++;
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProviderTierErrors.ArchivedImmutable(Id);
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw ProviderTierErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw ProviderTierErrors.InvalidStateTransition(Status, "validate");
    }
}
