using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Configuration.ConfigurationResolution;

public static class ConfigurationResolutionErrors
{
    public static DomainException IdMustNotBeEmpty() =>
        new DomainInvariantViolationException("ConfigurationResolutionId must not be empty.");
    public static DomainException IdMustBe64HexChars(string v) =>
        new DomainInvariantViolationException($"ConfigurationResolutionId must be 64 lowercase hex chars. Got: '{v}'.");
}
