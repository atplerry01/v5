using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemReconciliation.DiscrepancyDetection;

public static class DiscrepancyDetectionErrors
{
    public static DomainException DetectionIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("DiscrepancyDetectionId must not be null or empty.");

    public static DomainException DetectionIdMustBe64HexChars(string value) =>
        new DomainInvariantViolationException(
            $"DiscrepancyDetectionId must be exactly 64 lowercase hex characters. Got: '{value}'.");

    public static DomainException SourceReferenceMustNotBeEmpty() =>
        new DomainInvariantViolationException("DiscrepancyDetection source reference must not be null or empty.");

    public static DomainException DismissalReasonMustNotBeEmpty() =>
        new DomainInvariantViolationException("A reason must be provided when dismissing a discrepancy detection.");

    public static DomainException DetectionAlreadyClosed(DetectionStatus status) =>
        new DomainInvariantViolationException($"DiscrepancyDetection is already in '{status}' status and cannot be modified.");
}
