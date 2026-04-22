using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.AccessControl.Identity;

public static class IdentityErrors
{
    public static DomainException IdentityIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("IdentityId must not be null or empty.");

    public static DomainException IdentityIdMustBe64HexChars(string value) =>
        new DomainInvariantViolationException(
            $"IdentityId must be exactly 64 lowercase hex characters. Got: '{value}'.");

    public static DomainException IdentityNameMustNotBeEmpty() =>
        new DomainInvariantViolationException("Identity name must not be null or empty.");

    public static DomainException IdentityAlreadyDeactivated() =>
        new DomainInvariantViolationException("Identity is already deactivated.");

    public static DomainException IdentityAlreadySuspended() =>
        new DomainInvariantViolationException("Identity is already suspended.");

    public static DomainException CannotReactivatePermanentlyDeactivatedIdentity() =>
        new DomainInvariantViolationException("Cannot reactivate a permanently deactivated identity.");
}
