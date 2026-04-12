namespace Whycespace.Domain.BusinessSystem.Notification.Channel;

public sealed class ChannelAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ChannelId Id { get; private set; }
    public ChannelStatus Status { get; private set; }
    public ChannelType ChannelType { get; private set; }
    public int Version { get; private set; }

    private ChannelAggregate() { }

    public static ChannelAggregate Create(ChannelId id, ChannelType channelType)
    {
        var aggregate = new ChannelAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new ChannelRegisteredEvent(id, channelType);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Activate()
    {
        ValidateBeforeChange();

        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ChannelErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new ChannelActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Deactivate()
    {
        ValidateBeforeChange();

        var specification = new CanDeactivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ChannelErrors.InvalidStateTransition(Status, nameof(Deactivate));

        var @event = new ChannelDeactivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ChannelRegisteredEvent @event)
    {
        Id = @event.ChannelId;
        ChannelType = @event.ChannelType;
        Status = ChannelStatus.Draft;
        Version++;
    }

    private void Apply(ChannelActivatedEvent @event)
    {
        Status = ChannelStatus.Active;
        Version++;
    }

    private void Apply(ChannelDeactivatedEvent @event)
    {
        Status = ChannelStatus.Deactivated;
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
            throw ChannelErrors.MissingId();

        if (ChannelType == default)
            throw ChannelErrors.InvalidChannelType();

        if (!Enum.IsDefined(Status))
            throw ChannelErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
