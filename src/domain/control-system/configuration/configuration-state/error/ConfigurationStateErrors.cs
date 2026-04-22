using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Configuration.ConfigurationState;

public static class ConfigurationStateErrors
{
    public static DomainException IdMustNotBeEmpty() =>
        new DomainInvariantViolationException("ConfigurationStateId must not be empty.");
    public static DomainException IdMustBe64HexChars(string v) =>
        new DomainInvariantViolationException($"ConfigurationStateId must be 64 lowercase hex chars. Got: '{v}'.");
}
