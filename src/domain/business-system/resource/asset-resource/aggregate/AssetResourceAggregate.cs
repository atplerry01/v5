namespace Whycespace.Domain.BusinessSystem.Resource.AssetResource;

public sealed class AssetResourceAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public AssetResourceId Id { get; private set; }
    public AssetResourceStatus Status { get; private set; }
    public int Version { get; private set; }

    private AssetResourceAggregate() { }

    public static AssetResourceAggregate Create(AssetResourceId id)
    {
        var aggregate = new AssetResourceAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new AssetResourceCreatedEvent(id);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Activate()
    {
        ValidateBeforeChange();

        var specification = new CanActivateAssetResourceSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AssetResourceErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new AssetResourceActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Decommission()
    {
        ValidateBeforeChange();

        var specification = new CanDecommissionAssetResourceSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AssetResourceErrors.InvalidStateTransition(Status, nameof(Decommission));

        var @event = new AssetResourceDecommissionedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(AssetResourceCreatedEvent @event)
    {
        Id = @event.AssetResourceId;
        Status = AssetResourceStatus.Pending;
        Version++;
    }

    private void Apply(AssetResourceActivatedEvent @event)
    {
        Status = AssetResourceStatus.Active;
        Version++;
    }

    private void Apply(AssetResourceDecommissionedEvent @event)
    {
        Status = AssetResourceStatus.Decommissioned;
        Version++;
    }

    private void AddEvent(object @event)
    {
        _uncommittedEvents.Add(@event);
    }

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw AssetResourceErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw AssetResourceErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
