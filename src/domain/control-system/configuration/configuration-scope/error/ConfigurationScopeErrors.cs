using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Configuration.ConfigurationScope;

public static class ConfigurationScopeErrors
{
    public static DomainException IdMustNotBeEmpty() =>
        new DomainInvariantViolationException("ConfigurationScopeId must not be empty.");
    public static DomainException IdMustBe64HexChars(string v) =>
        new DomainInvariantViolationException($"ConfigurationScopeId must be 64 lowercase hex chars. Got: '{v}'.");
}
