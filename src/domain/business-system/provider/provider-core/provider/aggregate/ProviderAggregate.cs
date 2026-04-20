namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.Provider;

public sealed class ProviderAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ProviderId Id { get; private set; }
    public ProviderName Name { get; private set; }
    public ProviderType Type { get; private set; }
    public ProviderStatus Status { get; private set; }
    public ProviderReferenceCode? ReferenceCode { get; private set; }
    public int Version { get; private set; }

    private ProviderAggregate() { }

    public static ProviderAggregate Create(
        ProviderId id,
        ProviderName name,
        ProviderType type,
        ProviderReferenceCode? referenceCode = null)
    {
        var aggregate = new ProviderAggregate();

        var @event = new ProviderCreatedEvent(id, name, type, referenceCode);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Update(ProviderName name, ProviderType type)
    {
        EnsureMutable();

        var @event = new ProviderUpdatedEvent(Id, name, type);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProviderErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new ProviderActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        if (Status == ProviderStatus.Archived)
            throw ProviderErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new ProviderArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ProviderCreatedEvent @event)
    {
        Id = @event.ProviderId;
        Name = @event.Name;
        Type = @event.Type;
        ReferenceCode = @event.ReferenceCode;
        Status = ProviderStatus.Draft;
        Version++;
    }

    private void Apply(ProviderUpdatedEvent @event)
    {
        Name = @event.Name;
        Type = @event.Type;
        Version++;
    }

    private void Apply(ProviderActivatedEvent @event)
    {
        Status = ProviderStatus.Active;
        Version++;
    }

    private void Apply(ProviderArchivedEvent @event)
    {
        Status = ProviderStatus.Archived;
        Version++;
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProviderErrors.ArchivedImmutable(Id);
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw ProviderErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw ProviderErrors.InvalidStateTransition(Status, "validate");
    }
}
