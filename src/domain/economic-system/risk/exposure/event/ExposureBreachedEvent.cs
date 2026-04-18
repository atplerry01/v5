using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Risk.Exposure;

/// <summary>
/// Phase 6 T6.5 — raised when ExposureAggregate.DetectBreach finds that
/// the current TotalExposure exceeds the supplied threshold. The event is
/// the canonical signal for the risk → enforcement loop: a
/// runtime-side integration handler consumes it and dispatches a
/// DetectViolationCommand into the enforcement pipeline.
///
/// Breach detection is command-driven (caller supplies the threshold)
/// because ExposureAggregate does not persist thresholds — thresholds
/// live in the enforcement rule projection and are evaluated against the
/// aggregate's current TotalExposure at detection time.
/// </summary>
public sealed record ExposureBreachedEvent(
    ExposureId ExposureId,
    Amount TotalExposure,
    Amount Threshold,
    Currency Currency,
    Timestamp DetectedAt) : DomainEvent;
