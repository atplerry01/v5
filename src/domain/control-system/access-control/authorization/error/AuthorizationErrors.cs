using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.AccessControl.Authorization;

public static class AuthorizationErrors
{
    public static DomainException IdMustNotBeEmpty() =>
        new DomainInvariantViolationException("AuthorizationId must not be null or empty.");
    public static DomainException IdMustBe64HexChars(string value) =>
        new DomainInvariantViolationException($"AuthorizationId must be exactly 64 lowercase hex characters. Got: '{value}'.");
}
