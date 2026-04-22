using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Scheduling.ScheduleControl;

public static class ScheduleControlErrors
{
    public static DomainException IdMustNotBeEmpty() =>
        new DomainInvariantViolationException("ScheduleControlId must not be empty.");
    public static DomainException IdMustBe64HexChars(string v) =>
        new DomainInvariantViolationException($"ScheduleControlId must be 64 lowercase hex chars. Got: '{v}'.");
}
