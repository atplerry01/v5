using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Observability;

public sealed record ObservabilityArchivedEvent(
    ObservabilityId ObservabilityId,
    Timestamp ArchivedAt) : DomainEvent;
