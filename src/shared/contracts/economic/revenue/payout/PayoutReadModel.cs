namespace Whycespace.Shared.Contracts.Economic.Revenue.Payout;

public sealed record PayoutReadModel
{
    public Guid PayoutId { get; init; }
    public Guid DistributionId { get; init; }
    public string Status { get; init; } = string.Empty;
}
