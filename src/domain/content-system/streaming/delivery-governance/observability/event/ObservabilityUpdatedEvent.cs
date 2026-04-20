using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Observability;

public sealed record ObservabilityUpdatedEvent(
    ObservabilityId ObservabilityId,
    ObservabilitySnapshot PreviousSnapshot,
    ObservabilitySnapshot NewSnapshot,
    Timestamp UpdatedAt) : DomainEvent;
