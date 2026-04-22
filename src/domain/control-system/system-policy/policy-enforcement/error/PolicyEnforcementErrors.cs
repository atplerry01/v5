using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyEnforcement;

public static class PolicyEnforcementErrors
{
    public static DomainException EnforcementIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("PolicyEnforcementId must not be null or empty.");

    public static DomainException EnforcementIdMustBe64HexChars(string value) =>
        new DomainInvariantViolationException(
            $"PolicyEnforcementId must be exactly 64 lowercase hex characters. Got: '{value}'.");
}
