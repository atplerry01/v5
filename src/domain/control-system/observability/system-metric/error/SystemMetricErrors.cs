using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Observability.SystemMetric;

public static class SystemMetricErrors
{
    public static DomainException IdMustNotBeEmpty() => new DomainInvariantViolationException("SystemMetricId must not be empty.");
    public static DomainException IdMustBe64HexChars(string v) => new DomainInvariantViolationException($"SystemMetricId must be 64 lowercase hex chars. Got: '{v}'.");
}
