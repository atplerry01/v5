using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Configuration.ConfigurationDefinition;

public static class ConfigurationDefinitionErrors
{
    public static DomainException IdMustNotBeEmpty() =>
        new DomainInvariantViolationException("ConfigurationDefinitionId must not be empty.");
    public static DomainException IdMustBe64HexChars(string v) =>
        new DomainInvariantViolationException($"ConfigurationDefinitionId must be 64 lowercase hex chars. Got: '{v}'.");
}
