using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.ControlSystem.SystemReconciliation.DiscrepancyDetection;

namespace Whycespace.Domain.ControlSystem.SystemReconciliation.DiscrepancyResolution;

public sealed record DiscrepancyResolutionInitiatedEvent(
    DiscrepancyResolutionId Id,
    DiscrepancyDetectionId DetectionId,
    DateTimeOffset InitiatedAt) : DomainEvent;
