using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.AccessControl.Principal;

public static class PrincipalErrors
{
    public static DomainException PrincipalIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("PrincipalId must not be null or empty.");

    public static DomainException PrincipalIdMustBe64HexChars(string value) =>
        new DomainInvariantViolationException(
            $"PrincipalId must be exactly 64 lowercase hex characters. Got: '{value}'.");

    public static DomainException PrincipalNameMustNotBeEmpty() =>
        new DomainInvariantViolationException("Principal name must not be null or empty.");

    public static DomainException PrincipalAlreadyDeactivated() =>
        new DomainInvariantViolationException("Principal is already deactivated.");

    public static DomainException PrincipalAlreadySuspended() =>
        new DomainInvariantViolationException("Principal is already suspended.");

    public static DomainException RoleIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("RoleId must not be null or empty when assigning a role.");

    public static DomainException RoleAlreadyAssigned(string roleId) =>
        new DomainInvariantViolationException($"Role '{roleId}' is already assigned to this principal.");

    public static DomainException CannotModifyDeactivatedPrincipal() =>
        new DomainInvariantViolationException("Cannot modify a deactivated principal.");
}
