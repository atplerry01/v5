using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Observability.SystemTrace;

public sealed class SystemTraceAggregate : AggregateRoot
{
    public SystemTraceId Id { get; private set; }
    public string TraceId { get; private set; } = string.Empty;
    public string OperationName { get; private set; } = string.Empty;
    public string? ParentSpanId { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public SpanStatus? Status { get; private set; }

    private SystemTraceAggregate() { }

    public static SystemTraceAggregate Start(
        SystemTraceId id,
        string traceId,
        string operationName,
        DateTimeOffset startedAt,
        string? parentSpanId = null)
    {
        Guard.Against(string.IsNullOrEmpty(traceId), "TraceId must not be empty.");
        Guard.Against(string.IsNullOrEmpty(operationName), "OperationName must not be empty.");

        var aggregate = new SystemTraceAggregate();
        aggregate.RaiseDomainEvent(
            new SystemTraceSpanStartedEvent(id, traceId, operationName, startedAt, parentSpanId));
        return aggregate;
    }

    public void Complete(DateTimeOffset completedAt, SpanStatus status)
    {
        Guard.Against(CompletedAt.HasValue, "Span is already completed.");
        Guard.Against(completedAt <= StartedAt, "Span completedAt must be after startedAt.");
        RaiseDomainEvent(new SystemTraceSpanCompletedEvent(Id, completedAt, status));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case SystemTraceSpanStartedEvent e:
                Id = e.Id; TraceId = e.TraceId; OperationName = e.OperationName;
                StartedAt = e.StartedAt; ParentSpanId = e.ParentSpanId;
                break;
            case SystemTraceSpanCompletedEvent e:
                CompletedAt = e.CompletedAt; Status = e.Status;
                break;
        }
    }

    protected override void EnsureInvariants() =>
        Guard.Against(Id.Value is null, "SystemTrace span must have an Id.");
}
