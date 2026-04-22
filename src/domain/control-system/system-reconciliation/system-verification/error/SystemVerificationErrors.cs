using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemReconciliation.SystemVerification;

public static class SystemVerificationErrors
{
    public static DomainException VerificationIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("SystemVerificationId must not be null or empty.");

    public static DomainException VerificationIdMustBe64HexChars(string value) =>
        new DomainInvariantViolationException(
            $"SystemVerificationId must be exactly 64 lowercase hex characters. Got: '{value}'.");

    public static DomainException TargetSystemMustNotBeEmpty() =>
        new DomainInvariantViolationException("SystemVerification target system must not be null or empty.");

    public static DomainException FailureReasonMustNotBeEmpty() =>
        new DomainInvariantViolationException("A failure reason must be provided when recording a verification failure.");

    public static DomainException VerificationAlreadyTerminated(VerificationStatus status) =>
        new DomainInvariantViolationException($"SystemVerification is already in '{status}' status and cannot be modified.");
}
