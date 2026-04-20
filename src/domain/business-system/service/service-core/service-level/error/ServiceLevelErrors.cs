namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceLevel;

public static class ServiceLevelErrors
{
    public static ServiceLevelDomainException MissingId()
        => new("ServiceLevelId is required and must not be empty.");

    public static ServiceLevelDomainException MissingServiceDefinitionRef()
        => new("ServiceLevel must reference a service definition.");

    public static ServiceLevelDomainException InvalidStateTransition(ServiceLevelStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static ServiceLevelDomainException ArchivedImmutable(ServiceLevelId id)
        => new($"ServiceLevel '{id.Value}' is archived and cannot be mutated.");
}

public sealed class ServiceLevelDomainException : Exception
{
    public ServiceLevelDomainException(string message) : base(message) { }
}
