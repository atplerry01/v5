namespace Whycespace.Domain.BusinessSystem.Notification.Template;

public sealed class TemplateAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public TemplateId Id { get; private set; }
    public TemplateStatus Status { get; private set; }
    public TemplateContent Content { get; private set; }
    public int Version { get; private set; }

    private TemplateAggregate() { }

    public static TemplateAggregate Create(TemplateId id, TemplateContent content)
    {
        var aggregate = new TemplateAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new TemplateDraftedEvent(id, content);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Publish()
    {
        ValidateBeforeChange();

        var specification = new CanPublishSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TemplateErrors.InvalidStateTransition(Status, nameof(Publish));

        var @event = new TemplatePublishedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        ValidateBeforeChange();

        var specification = new CanArchiveSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TemplateErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new TemplateArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(TemplateDraftedEvent @event)
    {
        Id = @event.TemplateId;
        Content = @event.Content;
        Status = TemplateStatus.Draft;
        Version++;
    }

    private void Apply(TemplatePublishedEvent @event)
    {
        Status = TemplateStatus.Published;
        Version++;
    }

    private void Apply(TemplateArchivedEvent @event)
    {
        Status = TemplateStatus.Archived;
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
            throw TemplateErrors.MissingId();

        if (Content == default)
            throw TemplateErrors.InvalidContent();

        if (!Enum.IsDefined(Status))
            throw TemplateErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
