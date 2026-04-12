namespace Whycespace.Domain.BusinessSystem.Document.Version;

public sealed class VersionAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public VersionId Id { get; private set; }
    public VersionStatus Status { get; private set; }
    public VersionMetadata? Metadata { get; private set; }
    public int Version { get; private set; }

    private VersionAggregate() { }

    public static VersionAggregate Create(VersionId id)
    {
        var aggregate = new VersionAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new VersionCreatedEvent(id);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void AssignMetadata(VersionMetadata metadata)
    {
        ValidateBeforeChange();

        var isImmutable = new IsImmutableSpecification();
        if (isImmutable.IsSatisfiedBy(Status))
            throw VersionErrors.ImmutableAfterSuperseded();

        if (Status == VersionStatus.Released)
            throw VersionErrors.InvalidStateTransition(Status, nameof(AssignMetadata));

        Metadata = metadata;
    }

    public void Release()
    {
        ValidateBeforeChange();

        var specification = new CanReleaseSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw VersionErrors.InvalidStateTransition(Status, nameof(Release));

        if (Metadata is null)
            throw VersionErrors.MetadataRequired();

        var @event = new VersionReleasedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Supersede()
    {
        ValidateBeforeChange();

        var specification = new CanSupersedeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw VersionErrors.InvalidStateTransition(Status, nameof(Supersede));

        var @event = new VersionSupersededEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(VersionCreatedEvent @event)
    {
        Id = @event.VersionId;
        Status = VersionStatus.Draft;
        Version++;
    }

    private void Apply(VersionReleasedEvent @event)
    {
        Status = VersionStatus.Released;
        Version++;
    }

    private void Apply(VersionSupersededEvent @event)
    {
        Status = VersionStatus.Superseded;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw VersionErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw VersionErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-change validation gate
        // POLICY HOOK (to be enforced by runtime)
    }
}
