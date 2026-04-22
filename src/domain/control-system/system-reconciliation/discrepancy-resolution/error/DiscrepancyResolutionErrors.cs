using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemReconciliation.DiscrepancyResolution;

public static class DiscrepancyResolutionErrors
{
    public static DomainException ResolutionIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("DiscrepancyResolutionId must not be null or empty.");

    public static DomainException ResolutionIdMustBe64HexChars(string value) =>
        new DomainInvariantViolationException(
            $"DiscrepancyResolutionId must be exactly 64 lowercase hex characters. Got: '{value}'.");

    public static DomainException ResolutionNotesRequiredOnCompletion() =>
        new DomainInvariantViolationException("Resolution notes must be provided when completing a discrepancy resolution.");

    public static DomainException ResolutionAlreadyTerminated(ResolutionStatus status) =>
        new DomainInvariantViolationException($"DiscrepancyResolution is already in '{status}' status and cannot be modified.");
}
