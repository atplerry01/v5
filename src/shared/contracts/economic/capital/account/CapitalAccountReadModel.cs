namespace Whycespace.Shared.Contracts.Economic.Capital.Account;

public sealed record CapitalAccountReadModel
{
    public Guid AccountId { get; init; }
    public Guid OwnerId { get; init; }
    public string Currency { get; init; } = string.Empty;
    public decimal TotalBalance { get; init; }
    public decimal AvailableBalance { get; init; }
    public decimal ReservedBalance { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
