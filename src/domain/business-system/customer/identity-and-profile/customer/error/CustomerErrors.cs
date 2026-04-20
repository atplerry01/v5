namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Customer;

public static class CustomerErrors
{
    public static CustomerDomainException MissingId()
        => new("CustomerId is required and must not be empty.");

    public static CustomerDomainException InvalidStateTransition(CustomerStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static CustomerDomainException ArchivedImmutable(CustomerId id)
        => new($"Customer '{id.Value}' is archived and cannot be mutated.");

    public static CustomerDomainException AlreadyActive(CustomerId id)
        => new($"Customer '{id.Value}' is already active.");
}

public sealed class CustomerDomainException : Exception
{
    public CustomerDomainException(string message) : base(message) { }
}
