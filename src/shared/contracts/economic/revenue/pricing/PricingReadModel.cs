namespace Whycespace.Shared.Contracts.Economic.Revenue.Pricing;

public sealed record PricingReadModel
{
    public Guid PricingId { get; init; }
    public Guid ContractId { get; init; }
    public string Model { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTimeOffset DefinedAt { get; init; }
    public DateTimeOffset? LastAdjustedAt { get; init; }
    public int AdjustmentCount { get; init; }
}
