using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemReconciliation.ConsistencyCheck;

public static class ConsistencyCheckErrors
{
    public static DomainException CheckIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("ConsistencyCheckId must not be null or empty.");

    public static DomainException CheckIdMustBe64HexChars(string value) =>
        new DomainInvariantViolationException(
            $"ConsistencyCheckId must be exactly 64 lowercase hex characters. Got: '{value}'.");

    public static DomainException ScopeTargetMustNotBeEmpty() =>
        new DomainInvariantViolationException("ConsistencyCheck scope target must not be null or empty.");

    public static DomainException CheckAlreadyTerminated() =>
        new DomainInvariantViolationException("ConsistencyCheck has already completed or failed and cannot be transitioned.");
}
