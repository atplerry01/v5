using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.PolicyBinding;

public static class PolicyBindingErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("PolicyBindingId is required and must not be empty.");

    public static DomainException MissingServiceDefinitionRef()
        => new DomainInvariantViolationException("PolicyBinding must reference a service definition.");

    public static DomainException InvalidStateTransition(PolicyBindingStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException ArchivedImmutable(PolicyBindingId id)
        => new DomainInvariantViolationException($"PolicyBinding '{id.Value}' is archived and cannot be mutated.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("PolicyBinding has already been initialized.");
}
