using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemReconciliation.DiscrepancyDetection;

public sealed record DiscrepancyDetectedEvent(
    DiscrepancyDetectionId Id,
    DiscrepancyKind Kind,
    string SourceReference,
    DateTimeOffset DetectedAt) : DomainEvent;
