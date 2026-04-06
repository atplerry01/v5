using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.IntelligenceSystem.Observability.Trace;

public sealed record TraceCompletedEvent(Guid ObservabilityTraceId) : DomainEvent;
