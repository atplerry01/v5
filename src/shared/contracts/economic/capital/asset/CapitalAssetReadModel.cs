namespace Whycespace.Shared.Contracts.Economic.Capital.Asset;

public sealed record CapitalAssetReadModel
{
    public Guid AssetId { get; init; }
    public Guid OwnerId { get; init; }
    public decimal Value { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastValuedAt { get; init; }
}
