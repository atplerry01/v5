using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Observability.SystemTrace;

public static class SystemTraceErrors
{
    public static DomainException IdMustNotBeEmpty() =>
        new DomainInvariantViolationException("SystemTraceId must not be empty.");
    public static DomainException IdMustBe64HexChars(string v) =>
        new DomainInvariantViolationException($"SystemTraceId must be 64 lowercase hex chars. Got: '{v}'.");
}
