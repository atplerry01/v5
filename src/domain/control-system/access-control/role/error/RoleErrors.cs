using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.AccessControl.Role;

public static class RoleErrors
{
    public static DomainException IdMustNotBeEmpty() =>
        new DomainInvariantViolationException("RoleId must not be null or empty.");
    public static DomainException IdMustBe64HexChars(string value) =>
        new DomainInvariantViolationException($"RoleId must be exactly 64 lowercase hex characters. Got: '{value}'.");
}
