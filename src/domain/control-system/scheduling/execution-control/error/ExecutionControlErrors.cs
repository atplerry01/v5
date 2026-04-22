using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Scheduling.ExecutionControl;

public static class ExecutionControlErrors
{
    public static DomainException IdMustNotBeEmpty() =>
        new DomainInvariantViolationException("ExecutionControlId must not be empty.");
    public static DomainException IdMustBe64HexChars(string v) =>
        new DomainInvariantViolationException($"ExecutionControlId must be 64 lowercase hex chars. Got: '{v}'.");
}
