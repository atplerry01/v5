namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceConstraint;

public static class ServiceConstraintErrors
{
    public static ServiceConstraintDomainException MissingId()
        => new("ServiceConstraintId is required and must not be empty.");

    public static ServiceConstraintDomainException MissingServiceDefinitionRef()
        => new("ServiceConstraint must reference a service definition.");

    public static ServiceConstraintDomainException InvalidStateTransition(ConstraintStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static ServiceConstraintDomainException ArchivedImmutable(ServiceConstraintId id)
        => new($"ServiceConstraint '{id.Value}' is archived and cannot be mutated.");
}

public sealed class ServiceConstraintDomainException : Exception
{
    public ServiceConstraintDomainException(string message) : base(message) { }
}
