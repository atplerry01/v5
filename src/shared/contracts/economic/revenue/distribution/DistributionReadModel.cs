namespace Whycespace.Shared.Contracts.Economic.Revenue.Distribution;

public sealed record DistributionReadModel
{
    public Guid DistributionId { get; init; }
    public string SpvId { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public string Status { get; init; } = string.Empty;
}
