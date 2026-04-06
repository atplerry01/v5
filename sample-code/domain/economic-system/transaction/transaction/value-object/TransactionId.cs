using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

public sealed record TransactionId
{
    public Guid Value { get; }

    public TransactionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TransactionId cannot be empty.", nameof(value));

        Value = value;
    }

    public static TransactionId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));

    public override string ToString() => Value.ToString();
}
