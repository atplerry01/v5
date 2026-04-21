using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Account;

public static class AccountErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("AccountId is required and must not be empty.");

    public static DomainException MissingCustomerRef()
        => new DomainInvariantViolationException("Account must reference a customer.");

    public static DomainException InvalidStateTransition(AccountStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException ClosedImmutable(AccountId id)
        => new DomainInvariantViolationException($"Account '{id.Value}' is closed and cannot be mutated.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Account has already been initialized.");
}
