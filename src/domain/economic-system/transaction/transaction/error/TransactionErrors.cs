using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

public static class TransactionErrors
{
    public static DomainException TransactionNotInitiated() =>
        new("Transaction must be in Initiated status.");

    public static DomainException TransactionNotProcessing() =>
        new("Transaction must be in Processing status.");

    public static DomainException TransactionAlreadyCommitted() =>
        new("Transaction has already been committed.");

    public static DomainException TransactionAlreadyFailed() =>
        new("Transaction has already failed.");

    public static DomainException CannotFailCommittedTransaction() =>
        new("Cannot fail a committed transaction.");

    public static DomainException CannotCommitFailedTransaction() =>
        new("Cannot commit a failed transaction.");

    public static DomainException MissingKind() =>
        new("Transaction must declare a kind (e.g. expense, revenue).");

    public static DomainException MissingReferences() =>
        new("Transaction must carry at least one reference to an economic action.");

    public static DomainException InvalidReferenceKind(string kind) =>
        new($"Transaction reference kind '{kind}' is invalid.");
}
