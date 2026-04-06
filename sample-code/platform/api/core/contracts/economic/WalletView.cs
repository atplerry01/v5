namespace Whycespace.Platform.Api.Core.Contracts.Economic;

/// <summary>
/// Read-only wallet projection view.
/// Sourced from CQRS projections — no domain access, no aggregates.
/// </summary>
public sealed record WalletView
{
    public required Guid WalletId { get; init; }
    public required Guid OwnerId { get; init; }
    public required decimal Balance { get; init; }
    public required string Currency { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset? LastTransactionAt { get; init; }
}
