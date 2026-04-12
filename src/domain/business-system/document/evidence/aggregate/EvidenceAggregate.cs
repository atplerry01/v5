namespace Whycespace.Domain.BusinessSystem.Document.Evidence;

public sealed class EvidenceAggregate
{
    private readonly List<object> _uncommittedEvents = new();
    private readonly List<EvidenceArtifact> _artifacts = new();

    public EvidenceId Id { get; private set; }
    public EvidenceStatus Status { get; private set; }
    public IReadOnlyList<EvidenceArtifact> Artifacts => _artifacts.AsReadOnly();
    public int Version { get; private set; }

    private EvidenceAggregate() { }

    public static EvidenceAggregate Create(EvidenceId id)
    {
        var aggregate = new EvidenceAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new EvidenceCreatedEvent(id);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void AttachArtifact(EvidenceArtifact artifact)
    {
        ValidateBeforeChange();

        if (artifact is null)
            throw new ArgumentNullException(nameof(artifact));

        if (Status != EvidenceStatus.Captured)
            throw EvidenceErrors.CannotMutateAfterCapture();

        var @event = new EvidenceArtifactAttachedEvent(Id, artifact.ArtifactId, artifact.ArtifactType);
        Apply(@event, artifact);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Verify()
    {
        ValidateBeforeChange();

        var specification = new CanVerifySpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EvidenceErrors.InvalidStateTransition(Status, nameof(Verify));

        var @event = new EvidenceVerifiedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        ValidateBeforeChange();

        var specification = new CanArchiveEvidenceSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EvidenceErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new EvidenceArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(EvidenceCreatedEvent @event)
    {
        Id = @event.EvidenceId;
        Status = EvidenceStatus.Captured;
        Version++;
    }

    private void Apply(EvidenceArtifactAttachedEvent @event, EvidenceArtifact artifact)
    {
        _artifacts.Add(artifact);
        Version++;
    }

    private void Apply(EvidenceVerifiedEvent @event)
    {
        Status = EvidenceStatus.Verified;
        Version++;
    }

    private void Apply(EvidenceArchivedEvent @event)
    {
        Status = EvidenceStatus.Archived;
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
            throw EvidenceErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw EvidenceErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
