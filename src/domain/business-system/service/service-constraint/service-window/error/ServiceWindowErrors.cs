namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceWindow;

public static class ServiceWindowErrors
{
    public static ServiceWindowDomainException MissingId()
        => new("ServiceWindowId is required and must not be empty.");

    public static ServiceWindowDomainException MissingServiceDefinitionRef()
        => new("ServiceWindow must reference a service definition.");

    public static ServiceWindowDomainException InvalidStateTransition(ServiceWindowStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static ServiceWindowDomainException ArchivedImmutable(ServiceWindowId id)
        => new($"ServiceWindow '{id.Value}' is archived and cannot be mutated.");

    public static ServiceWindowDomainException ClosedWindowRequired()
        => new("ServiceWindow range requires a closed TimeWindow (EndsAt must be set).");
}

public sealed class ServiceWindowDomainException : Exception
{
    public ServiceWindowDomainException(string message) : base(message) { }
}
