namespace Whycespace.Domain.BusinessSystem.Subscription.Enrollment;

public sealed class EnrollmentAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public EnrollmentId Id { get; private set; }
    public EnrollmentRequest EnrollmentRequest { get; private set; }
    public EnrollmentStatus Status { get; private set; }
    public IReadOnlyList<object> UncommittedEvents => _uncommittedEvents.AsReadOnly();

    private EnrollmentAggregate() { }

    public static EnrollmentAggregate Request(EnrollmentId id, EnrollmentRequest request)
    {
        var aggregate = new EnrollmentAggregate();
        var @event = new EnrollmentRequestedEvent(id, request);
        aggregate.Apply(@event);
        aggregate.EnsureInvariants();
        aggregate._uncommittedEvents.Add(@event);
        return aggregate;
    }

    public void Activate()
    {
        if (!CanActivateSpecification.IsSatisfiedBy(Status))
            throw EnrollmentErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new EnrollmentActivatedEvent(Id);
        Apply(@event);
        EnsureInvariants();
        _uncommittedEvents.Add(@event);
    }

    public void Cancel()
    {
        if (!CanCancelSpecification.IsSatisfiedBy(Status))
            throw EnrollmentErrors.InvalidStateTransition(Status, nameof(Cancel));

        var @event = new EnrollmentCancelledEvent(Id);
        Apply(@event);
        EnsureInvariants();
        _uncommittedEvents.Add(@event);
    }

    private void Apply(EnrollmentRequestedEvent @event)
    {
        Id = @event.EnrollmentId;
        EnrollmentRequest = @event.Request;
        Status = EnrollmentStatus.Pending;
    }

    private void Apply(EnrollmentActivatedEvent _)
    {
        Status = EnrollmentStatus.Active;
    }

    private void Apply(EnrollmentCancelledEvent _)
    {
        Status = EnrollmentStatus.Cancelled;
    }

    private void EnsureInvariants()
    {
        if (Id == default)
            throw EnrollmentErrors.MissingId();

        if (EnrollmentRequest == default)
            throw EnrollmentErrors.MissingRequest();
    }
}
