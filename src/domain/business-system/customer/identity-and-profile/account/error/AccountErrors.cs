namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Account;

public static class AccountErrors
{
    public static AccountDomainException MissingId()
        => new("AccountId is required and must not be empty.");

    public static AccountDomainException MissingCustomerRef()
        => new("Account must reference a customer.");

    public static AccountDomainException InvalidStateTransition(AccountStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static AccountDomainException ClosedImmutable(AccountId id)
        => new($"Account '{id.Value}' is closed and cannot be mutated.");
}

public sealed class AccountDomainException : Exception
{
    public AccountDomainException(string message) : base(message) { }
}
