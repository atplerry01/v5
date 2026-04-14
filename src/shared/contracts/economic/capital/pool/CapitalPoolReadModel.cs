namespace Whycespace.Shared.Contracts.Economic.Capital.Pool;

public sealed record CapitalPoolReadModel
{
    public Guid PoolId { get; init; }
    public string Currency { get; init; } = string.Empty;
    public decimal TotalCapital { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
