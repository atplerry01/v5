namespace Whycespace.Domain.EconomicSystem.Transaction.Limit;

public sealed class LimitThreshold : Entity
{
    public Guid IdentityId { get; private set; }
    public string LimitType { get; private set; } = string.Empty;
    public decimal ThresholdValue { get; private set; }

    public static LimitThreshold Create(Guid id, Guid identityId, string limitType, decimal thresholdValue)
    {
        return new LimitThreshold
        {
            Id = id,
            IdentityId = identityId,
            LimitType = limitType,
            ThresholdValue = thresholdValue
        };
    }
}
