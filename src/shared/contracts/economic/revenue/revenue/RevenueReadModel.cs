namespace Whycespace.Shared.Contracts.Economic.Revenue.Revenue;

public sealed record RevenueReadModel
{
    public Guid RevenueId { get; init; }
    public string SpvId { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string SourceRef { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}
