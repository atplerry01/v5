using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

public static class TransactionErrors
{
    public static DomainException TransactionNotInitiated() =>
        new("Transaction must be in Initiated status.");

    public static DomainException TransactionAlreadyCompleted() =>
        new("Transaction has already been completed.");

    public static DomainException TransactionAlreadyFailed() =>
        new("Transaction has already failed.");

    public static DomainException CannotFailCompletedTransaction() =>
        new("Cannot fail a completed transaction.");

    public static DomainException CannotCompleteFailedTransaction() =>
        new("Cannot complete a failed transaction.");

    public static DomainException MissingInstructionReference() =>
        new("Transaction must reference an instruction.");

    public static DomainException MissingJournalReference() =>
        new("Transaction must produce a journal on completion.");

    public static DomainException CannotExecuteTwice() =>
        new("Transaction cannot be executed more than once.");
}
