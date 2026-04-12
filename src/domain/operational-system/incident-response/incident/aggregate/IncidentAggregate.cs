namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed class IncidentAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public IncidentId Id { get; private set; }
    public IncidentDescriptor Descriptor { get; private set; }
    public IncidentStatus Status { get; private set; }
    public IReadOnlyList<object> UncommittedEvents => _uncommittedEvents.AsReadOnly();

    private IncidentAggregate() { }

    public static IncidentAggregate Report(IncidentId id, IncidentDescriptor descriptor)
    {
        var aggregate = new IncidentAggregate();
        var @event = new IncidentReportedEvent(id, descriptor);
        aggregate.Apply(@event);
        aggregate.EnsureInvariants();
        aggregate._uncommittedEvents.Add(@event);
        return aggregate;
    }

    public void Investigate()
    {
        if (!CanInvestigateSpecification.IsSatisfiedBy(Status))
            throw IncidentErrors.InvalidStateTransition(Status, nameof(Investigate));

        var @event = new IncidentInvestigationStartedEvent(Id);
        Apply(@event);
        EnsureInvariants();
        _uncommittedEvents.Add(@event);
    }

    public void Resolve()
    {
        if (!CanResolveSpecification.IsSatisfiedBy(Status))
            throw IncidentErrors.InvalidStateTransition(Status, nameof(Resolve));

        var @event = new IncidentResolvedEvent(Id);
        Apply(@event);
        EnsureInvariants();
        _uncommittedEvents.Add(@event);
    }

    public void Close()
    {
        if (!CanCloseSpecification.IsSatisfiedBy(Status))
            throw IncidentErrors.InvalidStateTransition(Status, nameof(Close));

        var @event = new IncidentClosedEvent(Id);
        Apply(@event);
        EnsureInvariants();
        _uncommittedEvents.Add(@event);
    }

    private void Apply(IncidentReportedEvent @event)
    {
        Id = @event.IncidentId;
        Descriptor = @event.Descriptor;
        Status = IncidentStatus.Reported;
    }

    private void Apply(IncidentInvestigationStartedEvent _)
    {
        Status = IncidentStatus.Investigating;
    }

    private void Apply(IncidentResolvedEvent _)
    {
        Status = IncidentStatus.Resolved;
    }

    private void Apply(IncidentClosedEvent _)
    {
        Status = IncidentStatus.Closed;
    }

    private void EnsureInvariants()
    {
        if (Id == default)
            throw IncidentErrors.MissingId();

        if (Descriptor == default)
            throw IncidentErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw IncidentErrors.InvalidStateTransition(Status, "EnsureInvariants");
    }
}
