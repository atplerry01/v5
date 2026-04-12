namespace Whycespace.Domain.BusinessSystem.Execution.Completion;

public sealed class CompletionAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public CompletionId Id { get; private set; }
    public CompletionTargetId TargetId { get; private set; }
    public CompletionStatus Status { get; private set; }
    public string? FailureReason { get; private set; }
    public int Version { get; private set; }

    private CompletionAggregate() { }

    public static CompletionAggregate Create(CompletionId id, CompletionTargetId targetId)
    {
        var aggregate = new CompletionAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new CompletionCreatedEvent(id, targetId);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Complete()
    {
        ValidateBeforeChange();

        var specification = new CanCompleteSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CompletionErrors.InvalidStateTransition(Status, nameof(Complete));

        var @event = new CompletionCompletedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Fail(string reason)
    {
        ValidateBeforeChange();

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Failure reason must not be empty.", nameof(reason));

        var specification = new CanFailSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CompletionErrors.InvalidStateTransition(Status, nameof(Fail));

        var @event = new CompletionFailedEvent(Id, reason);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(CompletionCreatedEvent @event)
    {
        Id = @event.CompletionId;
        TargetId = @event.TargetId;
        Status = CompletionStatus.Pending;
        Version++;
    }

    private void Apply(CompletionCompletedEvent @event)
    {
        Status = CompletionStatus.Completed;
        Version++;
    }

    private void Apply(CompletionFailedEvent @event)
    {
        Status = CompletionStatus.Failed;
        FailureReason = @event.Reason;
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
            throw CompletionErrors.MissingId();

        if (TargetId == default)
            throw CompletionErrors.MissingTargetId();

        if (!Enum.IsDefined(Status))
            throw CompletionErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
