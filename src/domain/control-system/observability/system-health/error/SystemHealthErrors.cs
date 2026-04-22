using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Observability.SystemHealth;

public static class SystemHealthErrors
{
    public static DomainException HealthIdMustNotBeEmpty() =>
        new DomainInvariantViolationException("SystemHealthId must not be null or empty.");

    public static DomainException HealthIdMustBe64HexChars(string value) =>
        new DomainInvariantViolationException(
            $"SystemHealthId must be exactly 64 lowercase hex characters. Got: '{value}'.");

    public static DomainException ComponentNameMustNotBeEmpty() =>
        new DomainInvariantViolationException("SystemHealth component name must not be null or empty.");

    public static DomainException ReasonMustNotBeEmptyOnDegradation() =>
        new DomainInvariantViolationException("A reason must be provided when recording health degradation.");

    public static DomainException HealthAlreadyInStatus(HealthStatus status) =>
        new DomainInvariantViolationException($"SystemHealth component is already in '{status}' status.");
}
