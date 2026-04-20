using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Observability;

public sealed record ObservabilityFinalizedEvent(
    ObservabilityId ObservabilityId,
    Timestamp FinalizedAt) : DomainEvent;
