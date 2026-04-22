using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyDecision;

public static class PolicyDecisionErrors
{
    public static DomainException DecisionIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("PolicyDecisionId must not be null or empty.");

    public static DomainException DecisionIdMustBe64HexChars(string value) =>
        new DomainInvariantViolationException(
            $"PolicyDecisionId must be exactly 64 lowercase hex characters. Got: '{value}'.");
}
