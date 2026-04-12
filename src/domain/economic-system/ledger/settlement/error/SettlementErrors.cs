using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Settlement;

public static class SettlementErrors
{
    public static DomainException SettlementAlreadyCompleted()
        => new("Settlement has already been completed.");

    public static DomainException SettlementAlreadyFailed()
        => new("Settlement has already failed.");

    public static DomainException SettlementNotInitiated()
        => new("Settlement is not in an initiated state.");

    public static DomainException InvalidAmount()
        => new("Amount must be greater than zero.");

    public static DomainException MissingJournalReference()
        => new("Settlement must reference a journal.");

    public static DomainException MissingObligationReference()
        => new("Settlement must reference an obligation.");

    public static DomainException CannotCompleteFailedSettlement()
        => new("Cannot complete a failed settlement.");

    public static DomainException CannotFailCompletedSettlement()
        => new("Cannot fail a completed settlement.");

    public static DomainInvariantViolationException NegativeSettlementAmount()
        => new("Invariant violated: settlement amount cannot be negative or zero.");
}
