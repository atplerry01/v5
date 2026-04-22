using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.AccessControl.Permission;

public static class PermissionErrors
{
    public static DomainException IdMustNotBeEmpty() =>
        new DomainInvariantViolationException("PermissionId must not be null or empty.");
    public static DomainException IdMustBe64HexChars(string value) =>
        new DomainInvariantViolationException($"PermissionId must be exactly 64 lowercase hex characters. Got: '{value}'.");
}
