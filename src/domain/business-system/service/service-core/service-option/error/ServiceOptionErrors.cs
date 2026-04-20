namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceOption;

public static class ServiceOptionErrors
{
    public static ServiceOptionDomainException MissingId()
        => new("ServiceOptionId is required and must not be empty.");

    public static ServiceOptionDomainException MissingServiceDefinitionRef()
        => new("ServiceOption must reference a service definition.");

    public static ServiceOptionDomainException InvalidStateTransition(ServiceOptionStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static ServiceOptionDomainException ArchivedImmutable(ServiceOptionId id)
        => new($"ServiceOption '{id.Value}' is archived and cannot be mutated.");
}

public sealed class ServiceOptionDomainException : Exception
{
    public ServiceOptionDomainException(string message) : base(message) { }
}
