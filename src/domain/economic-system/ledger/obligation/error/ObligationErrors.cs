using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Obligation;

public static class ObligationErrors
{
    public static DomainException ObligationAlreadyFulfilled()
        => new("Obligation has already been fulfilled.");

    public static DomainException ObligationAlreadyCancelled()
        => new("Obligation has already been cancelled.");

    public static DomainException ObligationNotPending()
        => new("Obligation is not in a pending state.");

    public static DomainException InvalidAmount()
        => new("Amount must be greater than zero.");

    public static DomainException InvalidCounterparty()
        => new("Counterparty identifier cannot be empty.");

    public static DomainException CannotFulfilCancelledObligation()
        => new("Cannot fulfil a cancelled obligation.");

    public static DomainException CannotCancelFulfilledObligation()
        => new("Cannot cancel a fulfilled obligation.");

    public static DomainInvariantViolationException NegativeObligationAmount()
        => new("Invariant violated: obligation amount cannot be negative or zero.");
}
