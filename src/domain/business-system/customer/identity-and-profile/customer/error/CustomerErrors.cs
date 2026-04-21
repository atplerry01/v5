using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Customer;

public static class CustomerErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("CustomerId is required and must not be empty.");

    public static DomainException InvalidStateTransition(CustomerStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException ArchivedImmutable(CustomerId id)
        => new DomainInvariantViolationException($"Customer '{id.Value}' is archived and cannot be mutated.");

    public static DomainException AlreadyActive(CustomerId id)
        => new DomainInvariantViolationException($"Customer '{id.Value}' is already active.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Customer has already been initialized.");
}
