using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.IntelligenceSystem.Observability.Trace;

public sealed record TraceStartedEvent(Guid ObservabilityTraceId) : DomainEvent;
