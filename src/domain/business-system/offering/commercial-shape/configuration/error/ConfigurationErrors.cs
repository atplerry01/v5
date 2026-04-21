using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Configuration;

public static class ConfigurationErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("ConfigurationId is required and must not be empty.");

    public static DomainException InvalidStateTransition(ConfigurationStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException ArchivedImmutable(ConfigurationId id)
        => new DomainInvariantViolationException($"Configuration '{id.Value}' is archived and cannot be mutated.");

    public static DomainException OptionNotPresent(string key)
        => new DomainInvariantViolationException($"Configuration does not contain option '{key}'.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Configuration has already been initialized.");
}
