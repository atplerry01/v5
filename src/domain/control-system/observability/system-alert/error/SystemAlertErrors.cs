using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Observability.SystemAlert;

public static class SystemAlertErrors
{
    public static DomainException IdMustNotBeEmpty() =>
        new DomainInvariantViolationException("SystemAlertId must not be empty.");
    public static DomainException IdMustBe64HexChars(string v) =>
        new DomainInvariantViolationException($"SystemAlertId must be 64 lowercase hex chars. Got: '{v}'.");
}
