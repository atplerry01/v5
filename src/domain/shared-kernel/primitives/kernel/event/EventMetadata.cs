namespace Whycespace.Domain.SharedKernel.Primitives.Kernel;

public sealed record EventMetadata(EventId EventId, CorrelationId CorrelationId, CausationId CausationId, TraceId TraceId);
