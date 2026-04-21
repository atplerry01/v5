using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.PartyGovernance.Counterparty;

public static class CounterpartyErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("CounterpartyId is required and must not be empty.");

    public static DomainException MissingProfile()
        => new DomainInvariantViolationException("CounterpartyProfile is required and must not be null.");

    public static DomainException AlreadySuspended(CounterpartyId id)
        => new DomainInvariantViolationException($"Counterparty '{id.Value}' is already suspended.");

    public static DomainException AlreadyTerminated(CounterpartyId id)
        => new DomainInvariantViolationException($"Counterparty '{id.Value}' has already been terminated.");

    public static DomainException InvalidStateTransition(CounterpartyStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Counterparty has already been initialized.");
}
