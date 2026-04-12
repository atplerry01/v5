namespace Whycespace.Domain.BusinessSystem.Integration.Webhook;

public sealed class WebhookAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public WebhookId Id { get; private set; }
    public WebhookDefinition Definition { get; private set; }
    public WebhookStatus Status { get; private set; }
    public int Version { get; private set; }

    private WebhookAggregate() { }

    public static WebhookAggregate Create(WebhookId id, WebhookDefinition definition)
    {
        if (definition is null)
            throw new ArgumentNullException(nameof(definition));

        var aggregate = new WebhookAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new WebhookCreatedEvent(id, definition.EndpointId, definition.WebhookName);
        aggregate.Apply(@event, definition);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Activate()
    {
        ValidateBeforeChange();

        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw WebhookErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new WebhookActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Disable()
    {
        ValidateBeforeChange();

        var specification = new CanDisableSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw WebhookErrors.InvalidStateTransition(Status, nameof(Disable));

        var @event = new WebhookDisabledEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(WebhookCreatedEvent @event, WebhookDefinition definition)
    {
        Id = @event.WebhookId;
        Definition = definition;
        Status = WebhookStatus.Defined;
        Version++;
    }

    private void Apply(WebhookActivatedEvent @event)
    {
        Status = WebhookStatus.Active;
        Version++;
    }

    private void Apply(WebhookDisabledEvent @event)
    {
        Status = WebhookStatus.Disabled;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw WebhookErrors.MissingId();

        if (Definition is null)
            throw WebhookErrors.MissingDefinition();

        if (!Enum.IsDefined(Status))
            throw WebhookErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
