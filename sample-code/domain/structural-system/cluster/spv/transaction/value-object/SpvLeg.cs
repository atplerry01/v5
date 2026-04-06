namespace Whycespace.Domain.StructuralSystem.Cluster.Spv.Transaction;

public sealed record SpvLeg
{
    public Guid FromSpvId { get; init; }
    public Guid ToSpvId { get; init; }
    public Money Amount { get; init; } = null!;

    public static SpvLeg Create(Guid fromSpvId, Guid toSpvId, Money amount)
    {
        if (fromSpvId == Guid.Empty)
            throw new CrossSpvException("Source SPV required");

        if (toSpvId == Guid.Empty)
            throw new CrossSpvException("Target SPV required");

        if (fromSpvId == toSpvId)
            throw new CrossSpvException("Source and target SPV must differ");

        if (amount.IsZero || amount.IsNegative)
            throw new CrossSpvException("Leg amount must be positive");

        return new SpvLeg
        {
            FromSpvId = fromSpvId,
            ToSpvId = toSpvId,
            Amount = amount
        };
    }
}
