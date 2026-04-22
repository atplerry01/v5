using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.DecisionSystem.Risk.Exposure;

public static class ExposureErrors
{
    // ── Operation Errors ─────────────────────────────────────────

    public static DomainException MissingId() =>
        new("ExposureId is required and must not be empty.");

    public static DomainException InvalidExposureAmount() =>
        new("Exposure amount must be greater than zero.");

    public static DomainException ThresholdExceeded() =>
        new("Exposure exceeds the allowed threshold.");

    public static DomainException MissingSourceReference() =>
        new("Exposure must reference a valid source.");

    public static DomainException InvalidStateTransition(ExposureStatus current, string attemptedAction) =>
        new($"Cannot '{attemptedAction}' when current status is '{current}'.");

    public static DomainException AlreadyClosed() =>
        new("Cannot modify a closed exposure.");

    public static DomainException ReductionExceedsTotal() =>
        new("Cannot reduce exposure below zero.");

    // ── Invariant Violations ─────────────────────────────────────

    public static DomainInvariantViolationException ExposureMustBeNonNegative() =>
        new("Invariant violated: exposure must be >= 0.");
}
