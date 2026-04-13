namespace Whycespace.Domain.BusinessSystem.Document.ContractDocument;

public sealed class ContractDocumentAggregate
{
    private readonly List<object> _uncommittedEvents = new();
    private readonly List<DocumentSection> _sections = new();

    public ContractDocumentId Id { get; private set; }
    public ContractReferenceId ContractReferenceId { get; private set; }
    public ContractDocumentStatus Status { get; private set; }
    public IReadOnlyList<DocumentSection> Sections => _sections.AsReadOnly();
    public int Version { get; private set; }

    private ContractDocumentAggregate() { }

    public static ContractDocumentAggregate Create(ContractDocumentId id, ContractReferenceId contractReferenceId)
    {
        var aggregate = new ContractDocumentAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new ContractDocumentCreatedEvent(id, contractReferenceId);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void AddSection(DocumentSection section)
    {
        ValidateBeforeChange();

        if (section is null)
            throw new ArgumentNullException(nameof(section));

        var specification = new IsModifiableSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ContractDocumentErrors.CannotModifyAfterFinalization();

        var @event = new ContractDocumentSectionAddedEvent(Id, section.SectionId, section.Title);
        Apply(@event, section);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void MarkFinalized()
    {
        ValidateBeforeChange();

        var specification = new CanFinalizeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ContractDocumentErrors.InvalidStateTransition(Status, nameof(MarkFinalized));

        var @event = new ContractDocumentFinalizedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        ValidateBeforeChange();

        var specification = new CanArchiveContractDocumentSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ContractDocumentErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new ContractDocumentArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ContractDocumentCreatedEvent @event)
    {
        Id = @event.ContractDocumentId;
        ContractReferenceId = @event.ContractReferenceId;
        Status = ContractDocumentStatus.Draft;
        Version++;
    }

    private void Apply(ContractDocumentSectionAddedEvent @event, DocumentSection section)
    {
        _sections.Add(section);
        Version++;
    }

    private void Apply(ContractDocumentFinalizedEvent @event)
    {
        Status = ContractDocumentStatus.Finalized;
        Version++;
    }

    private void Apply(ContractDocumentArchivedEvent @event)
    {
        Status = ContractDocumentStatus.Archived;
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
            throw ContractDocumentErrors.MissingId();

        if (ContractReferenceId == default)
            throw ContractDocumentErrors.MissingContractReferenceId();

        if (!Enum.IsDefined(Status))
            throw ContractDocumentErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
