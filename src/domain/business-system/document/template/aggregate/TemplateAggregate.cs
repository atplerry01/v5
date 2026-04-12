namespace Whycespace.Domain.BusinessSystem.Document.Template;

public sealed class TemplateAggregate
{
    private readonly List<object> _uncommittedEvents = new();
    private readonly List<TemplateStructure> _structures = new();

    public TemplateId Id { get; private set; }
    public TemplateStatus Status { get; private set; }
    public IReadOnlyList<TemplateStructure> Structures => _structures.AsReadOnly();
    public int Version { get; private set; }

    private TemplateAggregate() { }

    public static TemplateAggregate Create(TemplateId id)
    {
        var aggregate = new TemplateAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new TemplateCreatedEvent(id);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void AddStructure(TemplateStructure structure)
    {
        ValidateBeforeChange();

        var isPublished = new IsPublishedSpecification();
        if (isPublished.IsSatisfiedBy(Status))
            throw TemplateErrors.ModificationAfterPublish();

        if (Status == TemplateStatus.Deprecated)
            throw TemplateErrors.InvalidStateTransition(Status, nameof(AddStructure));

        _structures.Add(structure);
    }

    public void Publish()
    {
        ValidateBeforeChange();

        var specification = new CanPublishSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TemplateErrors.InvalidStateTransition(Status, nameof(Publish));

        if (_structures.Count == 0)
            throw TemplateErrors.StructureRequired();

        var @event = new TemplatePublishedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Deprecate()
    {
        ValidateBeforeChange();

        var specification = new CanDeprecateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TemplateErrors.InvalidStateTransition(Status, nameof(Deprecate));

        var @event = new TemplateDeprecatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(TemplateCreatedEvent @event)
    {
        Id = @event.TemplateId;
        Status = TemplateStatus.Draft;
        Version++;
    }

    private void Apply(TemplatePublishedEvent @event)
    {
        Status = TemplateStatus.Published;
        Version++;
    }

    private void Apply(TemplateDeprecatedEvent @event)
    {
        Status = TemplateStatus.Deprecated;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw TemplateErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw TemplateErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-change validation gate
        // POLICY HOOK (to be enforced by runtime)
    }
}
