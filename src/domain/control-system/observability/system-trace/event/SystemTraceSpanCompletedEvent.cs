using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Observability.SystemTrace;

public sealed record SystemTraceSpanCompletedEvent(
    SystemTraceId Id,
    DateTimeOffset CompletedAt,
    SpanStatus Status) : DomainEvent;
