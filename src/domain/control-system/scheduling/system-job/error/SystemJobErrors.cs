using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Scheduling.SystemJob;

public static class SystemJobErrors
{
    public static DomainException IdMustNotBeEmpty() =>
        new DomainInvariantViolationException("SystemJobId must not be empty.");
    public static DomainException IdMustBe64HexChars(string v) =>
        new DomainInvariantViolationException($"SystemJobId must be 64 lowercase hex chars. Got: '{v}'.");
}
