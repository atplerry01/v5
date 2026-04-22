using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed class IncidentAggregate : AggregateRoot
{
    public IncidentId Id { get; private set; }
    public IncidentDescriptor Descriptor { get; private set; }
    public IncidentStatus Status { get; private set; }

    private IncidentAggregate() { }

    public static IncidentAggregate Report(IncidentId id, IncidentDescriptor descriptor)
    {
        var aggregate = new IncidentAggregate();
        aggregate.RaiseDomainEvent(new IncidentReportedEvent(id, descriptor));
        return aggregate;
    }

    public void Investigate()
    {
        if (!CanInvestigateSpecification.IsSatisfiedBy(Status))
            throw IncidentErrors.InvalidStateTransition(Status, nameof(Investigate));

        RaiseDomainEvent(new IncidentInvestigationStartedEvent(Id));
    }

    public void Resolve()
    {
        if (!CanResolveSpecification.IsSatisfiedBy(Status))
            throw IncidentErrors.InvalidStateTransition(Status, nameof(Resolve));

        RaiseDomainEvent(new IncidentResolvedEvent(Id));
    }

    public void Close()
    {
        if (!CanCloseSpecification.IsSatisfiedBy(Status))
            throw IncidentErrors.InvalidStateTransition(Status, nameof(Close));

        RaiseDomainEvent(new IncidentClosedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case IncidentReportedEvent e:
                Id = e.IncidentId;
                Descriptor = e.Descriptor;
                Status = IncidentStatus.Reported;
                break;
            case IncidentInvestigationStartedEvent:
                Status = IncidentStatus.Investigating;
                break;
            case IncidentResolvedEvent:
                Status = IncidentStatus.Resolved;
                break;
            case IncidentClosedEvent:
                Status = IncidentStatus.Closed;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw IncidentErrors.MissingId();

        if (Descriptor == default)
            throw IncidentErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw IncidentErrors.InvalidStateTransition(Status, "EnsureInvariants");
    }
}
