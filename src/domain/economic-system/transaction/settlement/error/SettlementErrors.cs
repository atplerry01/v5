using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Settlement;

public static class SettlementErrors
{
    public static DomainException InvalidAmount() =>
        new("Settlement amount must be greater than zero.");

    public static DomainException InvalidStateTransition(SettlementStatus from, SettlementStatus to) =>
        new($"Invalid settlement state transition: {from} -> {to}.");

    public static DomainException MissingSourceReference() =>
        new("Settlement must reference a valid source (ledger/transaction/capital).");

    public static DomainException AlreadyCompleted() =>
        new("Settlement has already been completed and cannot be mutated.");

    public static DomainException AlreadyFailed() =>
        new("Settlement has already been marked failed and cannot be mutated.");

    public static DomainInvariantViolationException NegativeAmount() =>
        new("Invariant violated: settlement amount cannot be negative.");

    public static DomainInvariantViolationException TerminalStateMutation() =>
        new("Invariant violated: settlement in a terminal state cannot be mutated.");
}
