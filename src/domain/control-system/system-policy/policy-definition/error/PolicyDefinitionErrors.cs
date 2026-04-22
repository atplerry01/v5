using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyDefinition;

public static class PolicyDefinitionErrors
{
    public static DomainException PolicyIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("PolicyId must not be null or empty.");

    public static DomainException PolicyIdMustBe64HexChars(string value) =>
        new DomainInvariantViolationException(
            $"PolicyId must be exactly 64 lowercase hex characters. Got: '{value}'.");

    public static DomainException ClassificationMustNotBeEmpty() =>
        new DomainInvariantViolationException("PolicyScope classification must not be null or empty.");

    public static DomainException ActionMaskMustNotBeEmpty() =>
        new DomainInvariantViolationException("PolicyScope actionMask must not be null or empty.");
}
