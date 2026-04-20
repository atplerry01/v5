namespace Whycespace.Domain.BusinessSystem.Agreement.PartyGovernance.Counterparty;

public static class CounterpartyErrors
{
    public static CounterpartyDomainException MissingId()
        => new("CounterpartyId is required and must not be empty.");

    public static CounterpartyDomainException MissingProfile()
        => new("CounterpartyProfile is required and must not be null.");

    public static CounterpartyDomainException AlreadySuspended(CounterpartyId id)
        => new($"Counterparty '{id.Value}' is already suspended.");

    public static CounterpartyDomainException AlreadyTerminated(CounterpartyId id)
        => new($"Counterparty '{id.Value}' has already been terminated.");

    public static CounterpartyDomainException InvalidStateTransition(CounterpartyStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class CounterpartyDomainException : Exception
{
    public CounterpartyDomainException(string message) : base(message) { }
}
