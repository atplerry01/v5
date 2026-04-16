namespace Whycespace.Shared.Contracts.Economic.Transaction.Charge;

public sealed record ChargeReadModel
{
    public Guid ChargeId { get; init; }
    public Guid TransactionId { get; init; }
    public string Type { get; init; } = string.Empty;
    public decimal BaseAmount { get; init; }
    public decimal ChargeAmount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CalculatedAt { get; init; }
    public DateTimeOffset? AppliedAt { get; init; }
}
