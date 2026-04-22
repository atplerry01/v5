using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Observability.SystemTrace;

public sealed record SystemTraceSpanStartedEvent(
    SystemTraceId Id,
    string TraceId,
    string OperationName,
    DateTimeOffset StartedAt,
    string? ParentSpanId) : DomainEvent;
