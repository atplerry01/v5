using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemReconciliation.DiscrepancyDetection;

public sealed record DiscrepancyDetectionDismissedEvent(
    DiscrepancyDetectionId Id,
    string Reason,
    DateTimeOffset DismissedAt) : DomainEvent;
