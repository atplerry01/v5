namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceDefinition;

public static class ServiceDefinitionErrors
{
    public static ServiceDefinitionDomainException MissingId()
        => new("ServiceDefinitionId is required and must not be empty.");

    public static ServiceDefinitionDomainException InvalidStateTransition(ServiceDefinitionStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static ServiceDefinitionDomainException ArchivedImmutable(ServiceDefinitionId id)
        => new($"ServiceDefinition '{id.Value}' is archived and cannot be mutated.");
}

public sealed class ServiceDefinitionDomainException : Exception
{
    public ServiceDefinitionDomainException(string message) : base(message) { }
}
