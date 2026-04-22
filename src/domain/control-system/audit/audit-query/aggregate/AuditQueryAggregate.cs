using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Audit.AuditQuery;

public sealed class AuditQueryAggregate : AggregateRoot
{
    public AuditQueryId Id { get; private set; }
    public string IssuedBy { get; private set; } = string.Empty;
    public QueryTimeRange TimeRange { get; private set; }
    public string? CorrelationFilter { get; private set; }
    public string? ActorFilter { get; private set; }
    public AuditQueryStatus Status { get; private set; }
    public int? ResultCount { get; private set; }

    private AuditQueryAggregate() { }

    public static AuditQueryAggregate Issue(
        AuditQueryId id,
        string issuedBy,
        QueryTimeRange timeRange,
        string? correlationFilter = null,
        string? actorFilter = null)
    {
        Guard.Against(string.IsNullOrEmpty(issuedBy), AuditQueryErrors.IssuedByMustNotBeEmpty().Message);

        var aggregate = new AuditQueryAggregate();
        aggregate.RaiseDomainEvent(new AuditQueryIssuedEvent(id, issuedBy, timeRange, correlationFilter, actorFilter));
        return aggregate;
    }

    public void Complete(int resultCount)
    {
        Guard.Against(Status == AuditQueryStatus.Completed, AuditQueryErrors.QueryAlreadyCompleted().Message);
        Guard.Against(Status != AuditQueryStatus.Issued, AuditQueryErrors.QueryMustBeIssuedBeforeCompletion().Message);

        RaiseDomainEvent(new AuditQueryCompletedEvent(Id, resultCount));
    }

    public void Expire()
    {
        Guard.Against(Status == AuditQueryStatus.Expired, AuditQueryErrors.QueryAlreadyExpired().Message);

        RaiseDomainEvent(new AuditQueryExpiredEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case AuditQueryIssuedEvent e:
                Id = e.Id;
                IssuedBy = e.IssuedBy;
                TimeRange = e.TimeRange;
                CorrelationFilter = e.CorrelationFilter;
                ActorFilter = e.ActorFilter;
                Status = AuditQueryStatus.Issued;
                break;
            case AuditQueryCompletedEvent e:
                Status = AuditQueryStatus.Completed;
                ResultCount = e.ResultCount;
                break;
            case AuditQueryExpiredEvent:
                Status = AuditQueryStatus.Expired;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Id.Value is null, "AuditQuery must have a non-empty Id.");
        Guard.Against(string.IsNullOrEmpty(IssuedBy), "AuditQuery must have a non-empty IssuedBy.");
    }
}
