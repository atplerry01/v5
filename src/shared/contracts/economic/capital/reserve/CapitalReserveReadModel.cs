namespace Whycespace.Shared.Contracts.Economic.Capital.Reserve;

public sealed record CapitalReserveReadModel
{
    public Guid ReserveId { get; init; }
    public Guid AccountId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset ReservedAt { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
}
