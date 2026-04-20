namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.PolicyBinding;

public static class PolicyBindingErrors
{
    public static PolicyBindingDomainException MissingId()
        => new("PolicyBindingId is required and must not be empty.");

    public static PolicyBindingDomainException MissingServiceDefinitionRef()
        => new("PolicyBinding must reference a service definition.");

    public static PolicyBindingDomainException InvalidStateTransition(PolicyBindingStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static PolicyBindingDomainException ArchivedImmutable(PolicyBindingId id)
        => new($"PolicyBinding '{id.Value}' is archived and cannot be mutated.");
}

public sealed class PolicyBindingDomainException : Exception
{
    public PolicyBindingDomainException(string message) : base(message) { }
}
