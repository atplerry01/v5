using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.AccessControl.AccessPolicy;

public static class AccessPolicyErrors
{
    public static DomainException AccessPolicyIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("AccessPolicyId must not be null or empty.");

    public static DomainException AccessPolicyIdMustBe64HexChars(string value) =>
        new DomainInvariantViolationException(
            $"AccessPolicyId must be exactly 64 lowercase hex characters. Got: '{value}'.");

    public static DomainException PolicyNameMustNotBeEmpty() =>
        new DomainInvariantViolationException("AccessPolicy name must not be null or empty.");

    public static DomainException PolicyScopeMustNotBeEmpty() =>
        new DomainInvariantViolationException("AccessPolicy scope must not be null or empty.");

    public static DomainException PolicyAlreadyActive() =>
        new DomainInvariantViolationException("AccessPolicy is already active.");

    public static DomainException PolicyAlreadyRetired() =>
        new DomainInvariantViolationException("AccessPolicy is already retired.");

    public static DomainException PolicyMustBeDraftBeforeActivation() =>
        new DomainInvariantViolationException("AccessPolicy must be in Draft status before it can be activated.");
}
