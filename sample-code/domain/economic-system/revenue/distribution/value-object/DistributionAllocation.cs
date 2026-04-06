namespace Whycespace.Domain.EconomicSystem.Revenue.Distribution;

public sealed record DistributionAllocation
{
    public Guid RecipientId { get; init; }
    public Money Amount { get; init; } = null!;

    public static DistributionAllocation Create(Guid recipientId, Money amount)
    {
        if (recipientId == Guid.Empty)
            throw new DistributionException("Invalid recipient");

        if (amount.IsZero || amount.IsNegative)
            throw new DistributionException("Invalid allocation amount");

        return new DistributionAllocation
        {
            RecipientId = recipientId,
            Amount = amount
        };
    }
}
