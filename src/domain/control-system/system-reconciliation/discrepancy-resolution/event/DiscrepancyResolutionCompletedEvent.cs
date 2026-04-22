using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemReconciliation.DiscrepancyResolution;

public sealed record DiscrepancyResolutionCompletedEvent(
    DiscrepancyResolutionId Id,
    ResolutionOutcome Outcome,
    string Notes,
    DateTimeOffset CompletedAt) : DomainEvent;
