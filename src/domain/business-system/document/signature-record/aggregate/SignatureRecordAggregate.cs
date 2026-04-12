namespace Whycespace.Domain.BusinessSystem.Document.SignatureRecord;

public sealed class SignatureRecordAggregate
{
    private readonly List<object> _uncommittedEvents = new();
    private readonly List<SignatureEntry> _entries = new();

    public SignatureRecordId Id { get; private set; }
    public SignatureRecordStatus Status { get; private set; }
    public IReadOnlyList<SignatureEntry> Entries => _entries.AsReadOnly();
    public int Version { get; private set; }

    private SignatureRecordAggregate() { }

    public static SignatureRecordAggregate Create(SignatureRecordId id)
    {
        var aggregate = new SignatureRecordAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new SignatureRecordCreatedEvent(id);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void AddEntry(SignatureEntry entry)
    {
        ValidateBeforeChange();

        var isVerified = new IsVerifiedSpecification();
        if (isVerified.IsSatisfiedBy(Status))
            throw SignatureRecordErrors.ModificationAfterVerification();

        _entries.Add(entry);
    }

    public void Verify()
    {
        ValidateBeforeChange();

        var specification = new CanVerifySpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SignatureRecordErrors.InvalidStateTransition(Status, nameof(Verify));

        if (_entries.Count == 0)
            throw SignatureRecordErrors.SignatureEntryRequired();

        var @event = new SignatureRecordVerifiedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        ValidateBeforeChange();

        var specification = new CanArchiveSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SignatureRecordErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new SignatureRecordArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(SignatureRecordCreatedEvent @event)
    {
        Id = @event.SignatureRecordId;
        Status = SignatureRecordStatus.Captured;
        Version++;
    }

    private void Apply(SignatureRecordVerifiedEvent @event)
    {
        Status = SignatureRecordStatus.Verified;
        Version++;
    }

    private void Apply(SignatureRecordArchivedEvent @event)
    {
        Status = SignatureRecordStatus.Archived;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw SignatureRecordErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw SignatureRecordErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-change validation gate
        // POLICY HOOK (to be enforced by runtime)
    }
}
