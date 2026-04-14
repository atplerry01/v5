namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

/// <summary>
/// Typed pointer to an economic action wrapped by a transaction.
/// Kind identifies the action type (see <see cref="TransactionKind"/>);
/// Id is the action's aggregate id. Transaction holds references only —
/// no business data from the linked action is pulled into this aggregate.
/// </summary>
public readonly record struct TransactionReference
{
    public string Kind { get; }
    public Guid Id { get; }

    public TransactionReference(string kind, Guid id)
    {
        if (string.IsNullOrWhiteSpace(kind))
            throw new ArgumentException("TransactionReference.Kind cannot be empty.", nameof(kind));
        if (id == Guid.Empty)
            throw new ArgumentException("TransactionReference.Id cannot be empty.", nameof(id));
        Kind = kind.Trim();
        Id = id;
    }

    public static TransactionReference Of(string kind, Guid id) => new(kind, id);
}
