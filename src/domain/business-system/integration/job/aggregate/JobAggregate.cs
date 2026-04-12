namespace Whycespace.Domain.BusinessSystem.Integration.Job;

public sealed class JobAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public JobId Id { get; private set; }
    public JobDescriptor Descriptor { get; private set; }
    public JobStatus Status { get; private set; }
    public int Version { get; private set; }

    private JobAggregate() { }

    public static JobAggregate Create(JobId id, JobDescriptor descriptor)
    {
        var aggregate = new JobAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new JobCreatedEvent(id, descriptor);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Start()
    {
        ValidateBeforeChange();

        var specification = new CanStartSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw JobErrors.InvalidStateTransition(Status, nameof(Start));

        var @event = new JobStartedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Complete()
    {
        ValidateBeforeChange();

        var specification = new CanCompleteSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw JobErrors.InvalidStateTransition(Status, nameof(Complete));

        var @event = new JobCompletedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Fail()
    {
        ValidateBeforeChange();

        var specification = new CanFailSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw JobErrors.InvalidStateTransition(Status, nameof(Fail));

        var @event = new JobFailedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(JobCreatedEvent @event)
    {
        Id = @event.JobId;
        Descriptor = @event.Descriptor;
        Status = JobStatus.Created;
        Version++;
    }

    private void Apply(JobStartedEvent @event)
    {
        Status = JobStatus.Running;
        Version++;
    }

    private void Apply(JobCompletedEvent @event)
    {
        Status = JobStatus.Completed;
        Version++;
    }

    private void Apply(JobFailedEvent @event)
    {
        Status = JobStatus.Failed;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw JobErrors.MissingId();

        if (Descriptor == default)
            throw JobErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw JobErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
