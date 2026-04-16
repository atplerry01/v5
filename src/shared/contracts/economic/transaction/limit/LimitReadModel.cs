namespace Whycespace.Shared.Contracts.Economic.Transaction.Limit;

public sealed record LimitReadModel
{
    public Guid LimitId { get; init; }
    public Guid AccountId { get; init; }
    public string Type { get; init; } = string.Empty;
    public decimal Threshold { get; init; }
    public string Currency { get; init; } = string.Empty;
    public decimal CurrentUtilization { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset DefinedAt { get; init; }
    public DateTimeOffset? LastCheckedAt { get; init; }
}
