namespace Whycespace.Domain.DecisionSystem.Compliance.Filing;

using Whycespace.Domain.DecisionSystem.Compliance.Filing;
using Whycespace.Domain.SharedKernel;

public sealed class ComplianceReportAggregate : AggregateRoot
{
    public Guid RegulationId { get; private set; }
    public Guid JurisdictionId { get; private set; }
    public string Title { get; private set; } = default!;
    public string ReportingPeriod { get; private set; } = default!;
    public ReportStatus Status { get; private set; } = default!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? SubmittedAt { get; private set; }
    private readonly List<FilingRecord> _filings = [];
    public IReadOnlyList<FilingRecord> Filings => _filings.AsReadOnly();

    private ComplianceReportAggregate() { }

    public static ComplianceReportAggregate Create(
        Guid reportId,
        Guid regulationId,
        Guid jurisdictionId,
        string title,
        string reportingPeriod)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException(ReportingErrors.InvalidTitle, "Report title is required.");

        var report = new ComplianceReportAggregate();
        var @event = new ReportCreatedEvent(
            reportId,
            regulationId,
            jurisdictionId,
            title,
            reportingPeriod);

        report.Apply(@event);
        report.RaiseDomainEvent(@event);
        return report;
    }

    public FilingRecord AddFiling(Guid filingId, string filingType, string referenceNumber, string submittedBy, DateTimeOffset timestamp)
    {
        if (Status == ReportStatus.Accepted || Status == ReportStatus.Rejected)
            throw new DomainException(ReportingErrors.InvalidTransition, "Cannot add filings to a finalized report.");

        var filing = FilingRecord.Create(filingId, Id, filingType, referenceNumber, submittedBy, timestamp);
        _filings.Add(filing);
        return filing;
    }

    public void Submit()
    {
        if (Status != ReportStatus.Draft && Status != ReportStatus.Revision)
            throw new DomainException(ReportingErrors.InvalidTransition, $"Cannot submit report in '{Status.Value}' status.");

        if (_filings.Count == 0)
            throw new DomainException(ReportingErrors.NoFilings, "Report must have at least one filing record.");

        var @event = new ReportSubmittedEvent(Id);
        Apply(@event);
        RaiseDomainEvent(@event);
    }

    public void Accept()
    {
        if (Status != ReportStatus.Submitted)
            throw new DomainException(ReportingErrors.InvalidTransition, "Only submitted reports can be accepted.");

        var @event = new ReportAcceptedEvent(Id);
        Apply(@event);
        RaiseDomainEvent(@event);
    }

    public void Reject(string reason)
    {
        if (Status != ReportStatus.Submitted)
            throw new DomainException(ReportingErrors.InvalidTransition, "Only submitted reports can be rejected.");

        var @event = new ReportRejectedEvent(Id, reason);
        Apply(@event);
        RaiseDomainEvent(@event);
    }

    public void RequestRevision(string reason)
    {
        if (Status != ReportStatus.Submitted)
            throw new DomainException(ReportingErrors.InvalidTransition, "Only submitted reports can be sent back for revision.");

        var @event = new ReportRevisionRequestedEvent(Id, reason);
        Apply(@event);
        RaiseDomainEvent(@event);
    }

    private void Apply(ReportCreatedEvent @event)
    {
        Id = @event.ReportId;
        RegulationId = @event.RegulationId;
        JurisdictionId = @event.JurisdictionId;
        Title = @event.Title;
        ReportingPeriod = @event.ReportingPeriod;
        Status = ReportStatus.Draft;
        CreatedAt = @event.OccurredAt;
    }

    private void Apply(ReportSubmittedEvent @event)
    {
        Status = ReportStatus.Submitted;
        SubmittedAt = @event.OccurredAt;
    }

    private void Apply(ReportAcceptedEvent _)
    {
        Status = ReportStatus.Accepted;
    }

    private void Apply(ReportRejectedEvent _)
    {
        Status = ReportStatus.Rejected;
    }

    private void Apply(ReportRevisionRequestedEvent _)
    {
        Status = ReportStatus.Revision;
    }
}
